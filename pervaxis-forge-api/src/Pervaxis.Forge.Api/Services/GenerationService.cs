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

    public GenerationService(
        ForgeDbContext db,
        IVerticalService verticalService,
        PrintGenerator printGenerator,
        IGitHubService gitHubService)
    {
        this.db = db;
        this.verticalService = verticalService;
        this.printGenerator = printGenerator;
        this.gitHubService = gitHubService;
    }

    public async Task<(byte[] Zip, GenerationResult Result)> GenerateAsync(GenerationRequest request, string generatedBy, CancellationToken ct = default)
    {
        var vertical = await verticalService.GetAsync(request.VerticalSlug, ct);
        if (vertical == null)
            throw new KeyNotFoundException($"Vertical '{request.VerticalSlug}' not found or inactive");

        var manifest = BuildManifest(request, vertical);
        await EnsureServiceNameAvailableAsync(vertical.Id, manifest.ServiceName, ct);
        var zipBytes = await printGenerator.GenerateAsync(manifest, vertical.CloudProvider, ct);

        var verticalEntity = await db.Verticals
            .Include(v => v.SourceControlConfig)
            .FirstOrDefaultAsync(v => v.Slug == request.VerticalSlug, ct);

        string? gitHubRepoUrl = null;
        var gitHubReposCreated = false;

        if (request.CreateGitHubRepo && verticalEntity?.SourceControlConfig != null)
        {
            var plainToken = verticalEntity.SourceControlConfig.AccessToken!;

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
        await WriteGeneratedServiceAsync(vertical.Id, manifest, generatedBy, ct);

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
                await EnsureServiceNameAvailableAsync(vertical.Id, manifest.ServiceName, ct);
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
            if (manifest.ServiceType == ServiceType.RestApi || manifest.ServiceType == ServiceType.GraphQL || manifest.ServiceType == ServiceType.Grpc)
            {
                var derivedNames = NamingConvention.DeriveDotNetNames(manifest.Product, manifest.ServiceName);
                @namespace = derivedNames.DotNetNamespace;
                projectName = derivedNames.ProjectFile;
            }
            else if (manifest.ServiceType == ServiceType.AngularShell)
            {
                var derivedNames = NamingConvention.DeriveAngularShellNames(manifest.Product, manifest.ServiceName);
                @namespace = derivedNames.AngularShellComponentName;
                projectName = derivedNames.AngularShellRoutePath;
            }
            else if (manifest.ServiceType == ServiceType.AngularMfe)
            {
                var derivedNames = NamingConvention.DeriveAngularMfeNames(manifest.Product, manifest.ServiceName);
                @namespace = derivedNames.AngularMfeComponentName;
                projectName = derivedNames.AngularMfeRoutePath;
            }
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

    public async Task<IReadOnlyList<GenerationAuditEntry>> GetAuditLogAsync(string verticalSlug, CancellationToken ct = default)
    {
        var logs = await db.GenerationLogs
            .Where(log => log.Vertical.Slug == verticalSlug && log.Vertical.IsActive)
            .OrderByDescending(log => log.CreatedAt)
            .ToListAsync(ct);

        var entries = logs.Select(log => new GenerationAuditEntry
        {
            Id = log.Id,
            ServiceCount = log.ServiceCount,
            GitHubReposCreated = log.GitHubReposCreated,
            InfrastructureDeployed = log.InfrastructureDeployed,
            CreatedBy = log.CreatedBy,
            CreatedAt = new DateTimeOffset(log.CreatedAt, TimeSpan.Zero),
            Manifest = log.Manifest.RootElement
        }).ToList();

        return entries.AsReadOnly();
    }

    public async Task<IReadOnlyList<GeneratedServiceResponse>> ListGeneratedServicesAsync(string verticalSlug, CancellationToken ct = default)
    {
        var vertical = await db.Verticals.AsNoTracking().FirstOrDefaultAsync(v => v.Slug == verticalSlug && v.IsActive, ct);
        if (vertical is null)
            throw new KeyNotFoundException($"Vertical '{verticalSlug}' not found or inactive");

        var services = await db.GeneratedServices
            .AsNoTracking()
            .Where(s => s.VerticalId == vertical.Id)
            .OrderByDescending(s => s.GeneratedAt)
            .ToListAsync(ct);

        return services.Select(s => new GeneratedServiceResponse
        {
            Id = s.Id,
            ServiceName = s.ServiceName,
            ServiceType = s.ServiceType,
            CloudProvider = s.CloudProvider,
            GeneratedAt = s.GeneratedAt,
            GeneratedBy = s.GeneratedBy
        }).ToList();
    }

    public async Task<(byte[] Zip, GeneratedServiceResponse Service)> RegenerateAsync(string verticalSlug, Guid serviceId, CancellationToken ct = default)
    {
        var vertical = await db.Verticals.AsNoTracking().FirstOrDefaultAsync(v => v.Slug == verticalSlug && v.IsActive, ct);
        if (vertical is null)
            throw new KeyNotFoundException($"Vertical '{verticalSlug}' not found or inactive");

        var service = await db.GeneratedServices
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == serviceId && s.VerticalId == vertical.Id, ct);
        if (service is null)
            throw new KeyNotFoundException($"Generated service '{serviceId}' not found for vertical '{verticalSlug}'");

        var manifest = JsonSerializer.Deserialize<ForgeManifest>(service.ManifestJson.RootElement.GetRawText())
            ?? throw new InvalidOperationException("Stored manifest could not be read.");

        var zipBytes = await printGenerator.GenerateAsync(manifest, service.CloudProvider, ct);
        return (zipBytes, new GeneratedServiceResponse
        {
            Id = service.Id,
            ServiceName = service.ServiceName,
            ServiceType = service.ServiceType,
            CloudProvider = service.CloudProvider,
            GeneratedAt = service.GeneratedAt,
            GeneratedBy = service.GeneratedBy
        });
    }

    private static ServiceType ParseServiceType(string type) => type.ToLowerInvariant() switch
    {
        "restapi" => ServiceType.RestApi,
        "angularshell" => ServiceType.AngularShell,
        "angularmfe" => ServiceType.AngularMfe,
        "graphql" => ServiceType.GraphQL,
        "grpc" => ServiceType.Grpc,
        _ => throw new InvalidOperationException($"Unsupported service type: '{type}'. Valid values: RestApi, AngularShell, AngularMfe, GraphQL, Grpc.")
    };

    private static ForgeManifest BuildManifest(GenerationRequest request, VerticalResponse vertical)
    {
        var serviceType = ParseServiceType(request.Type);
        var isBackend = serviceType == ServiceType.RestApi
            || serviceType == ServiceType.GraphQL
            || serviceType == ServiceType.Grpc;
        var isAngular = serviceType == ServiceType.AngularShell
            || serviceType == ServiceType.AngularMfe;

        var manifest = new ForgeManifest
        {
            Product = vertical.ComponentPrefix.ToLowerInvariant(),
            VerticalSlug = request.VerticalSlug,
            ServiceName = request.Name,
            ServiceType = serviceType,
            ComponentPrefix = vertical.ComponentPrefix,
            CloudProvider = vertical.CloudProvider,
            GenesisModules = isBackend ? request.GenesisModules : [],
            CanvasModules = isAngular ? request.CanvasModules : [],
            Metadata = new ManifestMetadata
            {
                Version = request.Version,
                CreatedAtUtc = DateTimeOffset.UtcNow
            }
        };

        if (isBackend && request.Database != null)
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

    private async Task WriteGeneratedServiceAsync(Guid verticalId, ForgeManifest manifest, string generatedBy, CancellationToken ct = default)
    {
        var manifestJson = JsonSerializer.Serialize(manifest);
        var jsonDoc = JsonDocument.Parse(manifestJson);

        db.GeneratedServices.Add(new GeneratedService
        {
            VerticalId = verticalId,
            ServiceName = manifest.ServiceName,
            ServiceType = manifest.ServiceType.ToString(),
            ManifestJson = jsonDoc,
            CloudProvider = manifest.CloudProvider,
            GeneratedAt = DateTimeOffset.UtcNow,
            GeneratedBy = generatedBy
        });

        await db.SaveChangesAsync(ct);
    }

    private async Task EnsureServiceNameAvailableAsync(Guid verticalId, string serviceName, CancellationToken ct)
    {
        var exists = await db.GeneratedServices.AnyAsync(s => s.VerticalId == verticalId && s.ServiceName == serviceName, ct);
        if (exists)
            throw new InvalidOperationException($"Service name '{serviceName}' already exists for this vertical.");
    }
}
