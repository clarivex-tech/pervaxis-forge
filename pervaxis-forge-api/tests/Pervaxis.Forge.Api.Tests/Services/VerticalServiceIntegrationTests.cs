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

using FluentAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Services;
using Xunit;

namespace Pervaxis.Forge.Api.Tests.Services;

// Hits real RDS. Requires RDS_TEST_CONNECTION env var pointing at a Postgres instance
// that has the InitialSchema migration applied. Excluded from CI via the
// `--filter "Category!=Integration"` flag in pr-check.yml.
[Trait("Category", "Integration")]
public sealed class VerticalServiceIntegrationTests : IAsyncLifetime
{
    private readonly string? _connectionString = Environment.GetEnvironmentVariable("RDS_TEST_CONNECTION");
    private readonly string _slug = $"itest-{Guid.NewGuid():N}"[..16];

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        if (string.IsNullOrEmpty(_connectionString)) return;
        await using var db = CreateContext();
        var v = await db.Verticals.FirstOrDefaultAsync(x => x.Slug == _slug);
        if (v is not null)
        {
            db.Verticals.Remove(v);
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task EnrollAsync_PersistsAggregate_AndRoundTripsEncryptedCredentials()
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        await using var db = CreateContext();
        var service = new VerticalService(db);

        var request = NewRequest(_slug, "ghp_secret_token", "arn:aws:iam::111111111111:role/X");

        var response = await service.EnrollAsync(request);

        response.Slug.Should().Be(_slug);
        response.Id.Should().NotBeEmpty();
        response.GitHubOrg.Should().Be("clarivex-tech");
        response.Environments.Should().BeEquivalentTo(["test", "accp", "prod"]);

        await using var verifyDb = CreateContext();
        var loaded = await verifyDb.Verticals
            .Include(v => v.CloudConfig)
            .Include(v => v.SourceControlConfig)
            .FirstAsync(v => v.Slug == _slug);

        loaded.SourceControlConfig!.AccessToken.Should().Be("ghp_secret_token",
            because: "EncryptedStringConverter should transparently decrypt on read");
        loaded.CloudConfig!.IamRoleArn.Should().Be("arn:aws:iam::111111111111:role/X");
    }

    [Fact]
    public async Task EnrollAsync_ThrowsSlugConflict_OnDuplicateSlug()
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        await using var db = CreateContext();
        var service = new VerticalService(db);

        await service.EnrollAsync(NewRequest(_slug, "t1", "arn:aws:iam::111111111111:role/X"));

        var act = async () => await service.EnrollAsync(
            NewRequest(_slug, "t2", "arn:aws:iam::222222222222:role/Y"));

        await act.Should().ThrowAsync<SlugConflictException>();
    }

    [Fact]
    public async Task GetUpdateUnenroll_RoundTripsAndSoftDeletes()
    {
        if (string.IsNullOrEmpty(_connectionString)) return;

        await using var db = CreateContext();
        var service = new VerticalService(db);

        await service.EnrollAsync(NewRequest(_slug, "tkn", "arn:aws:iam::111111111111:role/X"));

        var fetched = await service.GetAsync(_slug);
        fetched.Should().NotBeNull();
        fetched!.DisplayName.Should().Be("Test Vertical");

        var updated = await service.UpdateAsync(_slug, new UpdateVerticalRequest
        {
            DisplayName = "Updated Name",
            Description = "Updated description",
            OwnerTeam = "Team B",
            OwnerEmail = "b@e.com",
            TechDefaults = new VerticalTechDefaults { DefaultEnvironment = "accp" },
        });
        updated.Should().NotBeNull();
        updated!.DisplayName.Should().Be("Updated Name");

        var unenrolled = await service.UnenrollAsync(_slug);
        unenrolled.Should().BeTrue();

        var afterDelete = await service.GetAsync(_slug);
        afterDelete.Should().BeNull(because: "soft-deleted verticals are excluded from reads");
    }

    private ForgeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ForgeDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        return new ForgeDbContext(options, new EphemeralDataProtectionProvider());
    }

    private static VerticalEnrollmentRequest NewRequest(string slug, string token, string roleArn) => new()
    {
        Slug = slug,
        DisplayName = "Test Vertical",
        Description = "Integration test",
        OwnerTeam = "Team A",
        OwnerEmail = "a@e.com",
        CloudProvider = new CloudProviderConfig
        {
            Provider = "AWS",
            AwsAccountId = "111111111111",
            IamRoleArn = roleArn,
            DefaultRegion = "us-east-1",
        },
        SourceControl = new SourceControlConfig
        {
            Platform = "GitHub",
            GitHubOrg = "clarivex-tech",
            AccessToken = token,
            DefaultVisibility = "Private",
            DefaultBranchProtection = true,
        },
        TechDefaults = new VerticalTechDefaults { DefaultEnvironment = "test" },
    };

}
