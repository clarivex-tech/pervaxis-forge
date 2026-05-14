/*
 ************************************************************************
 * Copyright (C) 2026 Clarivex Technologies Private Limited
 * All Rights Reserved.
 *
 * NOTICE: All intellectual and technical concepts contained
 * herein are proprietary to Clarivex Technologies Private Limited
 * and may be covered by Indian and Foreign Patents,
 * patents in process, and are protected by trade secret or
 * copyright law. Dissemination of this information or reproduction
 * of this material is strictly forbidden unless prior written
 * permission is obtained from Clarivex Technologies Private Limited.
 *
 * Product:   Pervaxis Platform
 * Website:   https://clarivex.tech
 ************************************************************************
 */

using System.IO.Compression;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Data.Entities;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Models.Responses;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Naming;
using Pervaxis.Forge.Engine.Validation;

namespace Pervaxis.Forge.Api.Services;

public sealed class GenerationService : IGenerationService
{
    private readonly ForgeDbContext db;
    private readonly IVerticalService verticalService;
    private readonly PrintGenerator printGenerator;
    private readonly IGitHubService gitHubService;
    private readonly IDataProtectionProvider dataProtectionProvider;

    public GenerationService(
        ForgeDbContext db,
        IVerticalService verticalService,
        PrintGenerator printGenerator,
        IGitHubService gitHubService,
        IDataProtectionProvider dataProtectionProvider)
    {
        this.db = db;
        this.verticalService = verticalService;
        this.printGenerator = printGenerator;
        this.gitHubService = gitHubService;
        this.dataProtectionProvider = dataProtectionProvider;
    }

    public async Task<(byte[] Zip, GenerationResult Result)> GenerateAsync(GenerationRequest request, CancellationToken ct = default)
    {
        var vertical = await verticalService.GetAsync(request.VerticalSlug, ct);
        if (vertical == null)
            throw new KeyNotFoundException($"Vertical '{request.VerticalSlug}' not found or inactive");

        var manifest = BuildManifest(request, vertical);
        var zipBytes = await printGenerator.GenerateAsync(manifest, vertical.CloudProvider, ct);

        var verticalEntity = await db.Verticals
            .Include(v => v.SourceControlConfig)
            .FirstOrDefaultAsync(v => v.Slug == request.VerticalSlug, ct);

        string? gitHubRepoUrl = null;
        var gitHubReposCreated = false;

        if (request.CreateGitHubRepo && verticalEntity?.SourceControlConfig != null)
        {
            var protector = dataProtectionProvider.CreateProtector("VerticalSourceControl");
            var plainToken = protector.Unprotect(verticalEntity.SourceControlConfig.AccessToken!);

            gitHubRepoUrl = await gitHubService.CreateRepositoryAsync(
                plainToken,
                verticalEntity.SourceControlConfig.GitHubOrg!,
                request.Name,
                request.Description,
                isPrivate: true,
                ct);

            await gitHubService.ConfigureBranchProtectionAsync(
                plainToken,
                verticalEntity.SourceControlConfig.GitHubOrg!,
                request.Name,
                "main",
                ct);

            await gitHubService.PushInitialCommitAsync(
                gitHubRepoUrl,
                plainToken,
                zipBytes,
                "Pervaxis Forge",
                "forge@clarivex.tech",
                ct);

            gitHubReposCreated = true;
        }

        await WriteGenerationLogAsync(vertical.Id, manifest, 1, gitHubReposCreated, ct);

        var result = new GenerationResult
        {
            ServiceName = request.Name,
            VerticalSlug = request.VerticalSlug,
            GitHubRepoUrl = gitHubRepoUrl,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        return (zipBytes, result);
    }

    public async Task<(byte[] Zip, BatchGenerationResult Result)> GenerateBatchAsync(BatchGenerationRequest request, CancellationToken ct = default)
    {
        var vertical = await verticalService.GetAsync(request.VerticalSlug, ct);
        if (vertical == null)
            throw new KeyNotFoundException($"Vertical '{request.VerticalSlug}' not found or inactive");

        var results = new List<GenerationResult>();
        var serviceZips = new List<(string Name, byte[] Data)>();
        var succeededCount = 0;

        foreach (var serviceSpec in request.Services)
        {
            try
            {
                var fullRequest = new GenerationRequest
                {
                    VerticalSlug = request.VerticalSlug,
                    Name = serviceSpec.Name,
                    DisplayName = serviceSpec.DisplayName,
                    Description = serviceSpec.Description,
                    Version = serviceSpec.Version,
                    Type = serviceSpec.Type,
                    GenesisModules = serviceSpec.GenesisModules,
                    Database = serviceSpec.Database,
                    Queues = serviceSpec.Queues,
                    Metadata = serviceSpec.Metadata
                };

                var manifest = BuildManifest(fullRequest, vertical);
                var zipBytes = await printGenerator.GenerateAsync(manifest, vertical.CloudProvider, ct);

                serviceZips.Add((serviceSpec.Name, zipBytes));
                results.Add(new GenerationResult
                {
                    ServiceName = serviceSpec.Name,
                    VerticalSlug = request.VerticalSlug,
                    GeneratedAt = DateTimeOffset.UtcNow
                });

                succeededCount++;
            }
            catch (InvalidOperationException)
            {
                continue;
            }
        }

        var combinedZip = CreateCombinedZip(serviceZips);

        await WriteGenerationLogAsync(vertical.Id, new ForgeManifest
        {
            Product = vertical.ComponentPrefix.ToLowerInvariant(),
            VerticalSlug = request.VerticalSlug,
            ServiceName = "batch",
            ServiceType = ServiceType.RestApi,
            ComponentPrefix = vertical.ComponentPrefix,
            CloudProvider = vertical.CloudProvider
        }, succeededCount, false, ct);

        var batchResult = new BatchGenerationResult
        {
            VerticalSlug = request.VerticalSlug,
            Results = results.AsReadOnly(),
            TotalServices = request.Services.Count,
            SucceededCount = succeededCount,
            FailedCount = request.Services.Count - succeededCount,
            GeneratedAt = DateTimeOffset.UtcNow
        };

        return (combinedZip, batchResult);
    }

    public async Task<ValidationPreviewResult> ValidateAsync(GenerationRequest request, CancellationToken ct = default)
    {
        var vertical = await verticalService.GetAsync(request.VerticalSlug, ct);
        if (vertical == null)
            throw new KeyNotFoundException($"Vertical '{request.VerticalSlug}' not found or inactive");

        var manifest = BuildManifest(request, vertical);

        var validator = new ManifestValidator();
        var validationResult = validator.Validate(manifest);

        string? @namespace = null;
        string? projectName = null;

        if (validationResult.IsValid)
        {
            var derivedNames = NamingConvention.DeriveDotNetNames(manifest.Product, manifest.ServiceName);
            @namespace = derivedNames.DotNetNamespace;
            projectName = derivedNames.ProjectFile;
        }

        return new ValidationPreviewResult
        {
            IsValid = validationResult.IsValid,
            Errors = validationResult.Errors,
            ServiceName = request.Name,
            Namespace = @namespace,
            ProjectName = projectName
        };
    }

    private static ForgeManifest BuildManifest(GenerationRequest request, VerticalResponse vertical)
    {
        var manifest = new ForgeManifest
        {
            Product = vertical.ComponentPrefix.ToLowerInvariant(),
            VerticalSlug = request.VerticalSlug,
            ServiceName = request.Name,
            ServiceType = ServiceType.RestApi,
            ComponentPrefix = vertical.ComponentPrefix,
            CloudProvider = vertical.CloudProvider,
            GenesisModules = request.GenesisModules,
            Metadata = new ManifestMetadata
            {
                Version = request.Version,
                CreatedAtUtc = DateTimeOffset.UtcNow
            }
        };

        if (request.Database != null)
        {
            manifest = manifest with
            {
                Database = new DatabaseConfig
                {
                    Engine = request.Database.Engine,
                    Name = request.Database.Schema
                }
            };
        }

        return manifest;
    }

    private static byte[] CreateCombinedZip(List<(string Name, byte[] Data)> serviceZips)
    {
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var (name, data) in serviceZips)
            {
                var entry = archive.CreateEntry($"{name}.zip");
                using (var entryStream = entry.Open())
                {
                    entryStream.Write(data, 0, data.Length);
                }
            }
        }

        return memoryStream.ToArray();
    }

    private async Task WriteGenerationLogAsync(Guid verticalId, ForgeManifest manifest, int serviceCount, bool gitHubReposCreated, CancellationToken ct = default)
    {
        var manifestJson = JsonSerializer.Serialize(manifest);
        var jsonDoc = JsonDocument.Parse(manifestJson);

        var log = new GenerationLog
        {
            VerticalId = verticalId,
            Manifest = jsonDoc,
            ServiceCount = serviceCount,
            InfrastructureDeployed = false,
            GitHubReposCreated = gitHubReposCreated,
            CreatedBy = "forge-api",
            CreatedAt = DateTime.UtcNow
        };

        db.GenerationLogs.Add(log);
        await db.SaveChangesAsync(ct);
    }
}
