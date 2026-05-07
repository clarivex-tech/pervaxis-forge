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

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Data.Entities;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Models.Responses;

using EntityTechDefaults = Pervaxis.Forge.Api.Data.Entities.VerticalTechDefaults;

namespace Pervaxis.Forge.Api.Services;

public sealed class VerticalService(ForgeDbContext db) : IVerticalService
{
    public async Task<VerticalResponse> EnrollAsync(
        VerticalEnrollmentRequest request,
        CancellationToken ct = default)
    {
        if (await db.Verticals.AnyAsync(v => v.Slug == request.Slug, ct))
            throw new SlugConflictException(request.Slug);

        var vertical = new Vertical
        {
            Slug = request.Slug,
            DisplayName = request.DisplayName,
            Description = request.Description,
            OwnerTeam = request.OwnerTeam,
            OwnerEmail = request.OwnerEmail,
            CloudConfig = new VerticalCloudConfig
            {
                Provider = request.CloudProvider.Provider,
                AwsAccountId = request.CloudProvider.AwsAccountId,
                IamRoleArn = request.CloudProvider.IamRoleArn,
                DefaultRegion = request.CloudProvider.DefaultRegion,
            },
            SourceControlConfig = new VerticalSourceControlConfig
            {
                Platform = request.SourceControl.Platform,
                GitHubOrg = request.SourceControl.GitHubOrg,
                AccessToken = request.SourceControl.AccessToken,
                DefaultVisibility = request.SourceControl.DefaultVisibility,
                DefaultBranchProtection = request.SourceControl.DefaultBranchProtection,
            },
            TechDefaults = new EntityTechDefaults
            {
                Environments = [.. request.TechDefaults.Environments],
                DefaultEnvironment = request.TechDefaults.DefaultEnvironment,
                GenerateTerraform = request.TechDefaults.GenerateTerraform,
                GenerateCdk = request.TechDefaults.GenerateCdk,
                DefaultDbEngine = request.TechDefaults.DefaultDbEngine,
            },
        };

        db.Verticals.Add(vertical);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Race window between pre-check and save — translate the unique violation to a conflict.
            throw new SlugConflictException(request.Slug);
        }

        return MapToResponse(vertical);
    }

    public async Task<IReadOnlyList<VerticalSummaryResponse>> ListAsync(CancellationToken ct = default)
    {
        return await db.Verticals
            .AsNoTracking()
            .Where(v => v.IsActive)
            .OrderBy(v => v.Slug)
            .Select(v => new VerticalSummaryResponse
            {
                Id = v.Id,
                Slug = v.Slug,
                DisplayName = v.DisplayName,
                Description = v.Description,
                CloudProvider = v.CloudConfig != null ? v.CloudConfig.Provider : "Unknown",
                SourceControl = v.SourceControlConfig != null ? v.SourceControlConfig.Platform : "Unknown",
                ServiceCount = v.GenerationLogs.Sum(g => g.ServiceCount),
                EnrolledAt = new DateTimeOffset(v.CreatedAt, TimeSpan.Zero),
            })
            .ToListAsync(ct);
    }

    public async Task<VerticalResponse?> GetAsync(string slug, CancellationToken ct = default)
    {
        var vertical = await db.Verticals
            .AsNoTracking()
            .Include(v => v.CloudConfig)
            .Include(v => v.SourceControlConfig)
            .Include(v => v.TechDefaults)
            .FirstOrDefaultAsync(v => v.Slug == slug && v.IsActive, ct);

        return vertical is null ? null : MapToResponse(vertical);
    }

    public async Task<VerticalResponse?> UpdateAsync(
        string slug,
        UpdateVerticalRequest request,
        CancellationToken ct = default)
    {
        var vertical = await db.Verticals
            .Include(v => v.CloudConfig)
            .Include(v => v.SourceControlConfig)
            .Include(v => v.TechDefaults)
            .FirstOrDefaultAsync(v => v.Slug == slug && v.IsActive, ct);

        if (vertical is null) return null;

        vertical.DisplayName = request.DisplayName;
        vertical.Description = request.Description;
        vertical.OwnerTeam = request.OwnerTeam;
        vertical.OwnerEmail = request.OwnerEmail;
        vertical.UpdatedAt = DateTime.UtcNow;

        if (vertical.TechDefaults is not null)
        {
            vertical.TechDefaults.Environments = [.. request.TechDefaults.Environments];
            vertical.TechDefaults.DefaultEnvironment = request.TechDefaults.DefaultEnvironment;
            vertical.TechDefaults.GenerateTerraform = request.TechDefaults.GenerateTerraform;
            vertical.TechDefaults.GenerateCdk = request.TechDefaults.GenerateCdk;
            vertical.TechDefaults.DefaultDbEngine = request.TechDefaults.DefaultDbEngine;
            vertical.TechDefaults.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);

        return MapToResponse(vertical);
    }

    public async Task<bool> UnenrollAsync(string slug, CancellationToken ct = default)
    {
        var vertical = await db.Verticals
            .FirstOrDefaultAsync(v => v.Slug == slug && v.IsActive, ct);

        if (vertical is null) return false;

        vertical.IsActive = false;
        vertical.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return true;
    }

    public static VerticalResponse MapToResponse(Vertical vertical) => new()
    {
        Id = vertical.Id,
        Slug = vertical.Slug,
        DisplayName = vertical.DisplayName,
        CloudProvider = vertical.CloudConfig?.Provider ?? "Unknown",
        SourceControl = vertical.SourceControlConfig?.Platform ?? "Unknown",
        GitHubOrg = vertical.SourceControlConfig?.GitHubOrg ?? string.Empty,
        Environments = vertical.TechDefaults?.Environments ?? [],
        EnrolledAt = new DateTimeOffset(vertical.CreatedAt, TimeSpan.Zero),
    };

    private static bool IsUniqueViolation(DbUpdateException ex) =>
        ex.InnerException is PostgresException pg && pg.SqlState == PostgresErrorCodes.UniqueViolation;
}
