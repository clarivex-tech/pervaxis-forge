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
using FluentAssertions;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Validation;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public class MultiTenancyGenerationTests
{
    private readonly PrintGenerator generator = new();
    private readonly ManifestValidator validator = new();

    private static ForgeManifest Manifest(bool multiTenancy, bool withCognito = true, bool allowApiKey = false, string engine = "postgresql") => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "CLV",
        CloudProvider = "AWS",
        Database = new DatabaseConfig { Engine = engine, Name = "billing" },
        MultiTenancy = multiTenancy,
        Cognito = withCognito
            ? new CognitoConfig
            {
                UserPoolId = "us-east-1_AbC123xyz",
                ClientIds = ["1h57kf5cpq17m0eml12EXAMPLE"],
                AllowApiKey = allowApiKey,
            }
            : null,
    };

    private async Task<Dictionary<string, string>> GenerateAsync(ForgeManifest manifest)
    {
        var zipBytes = await generator.GenerateAsync(manifest, "AWS");
        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        var files = new Dictionary<string, string>();
        foreach (var entry in archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name)))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName.Replace('\\', '/')] = reader.ReadToEnd();
        }

        return files;
    }

    private static string Single(Dictionary<string, string> files, string suffix) =>
        files.Single(f => f.Key.EndsWith(suffix, StringComparison.Ordinal)).Value;

    [Fact]
    public void Validator_AcceptsPostgreSqlWithCognito()
    {
        validator.Validate(Manifest(multiTenancy: true)).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validator_RejectsTenancyWithoutDatabase()
    {
        var manifest = Manifest(multiTenancy: true) with { Database = null };

        validator.Validate(manifest).Errors.Should().Contain(e => e.Contains("Database"));
    }

    [Fact]
    public void Validator_RejectsTenancyOnAnotherDatabaseEngine()
    {
        validator.Validate(Manifest(multiTenancy: true, engine: "mysql")).Errors
            .Should().Contain(e => e.Contains("PostgreSQL"));
    }

    [Fact]
    public void Validator_RejectsTenancyWithoutCognito()
    {
        validator.Validate(Manifest(multiTenancy: true, withCognito: false)).Errors
            .Should().Contain(e => e.Contains("Cognito"));
    }

    [Fact]
    public void Validator_RejectsTenancyWithApiKeys()
    {
        validator.Validate(Manifest(multiTenancy: true, allowApiKey: true)).Errors
            .Should().Contain(e => e.Contains("API keys"));
    }

    [Fact]
    public void Validator_DoesNotConstrainServicesWithoutTenancy()
    {
        validator.Validate(Manifest(multiTenancy: false, withCognito: false, engine: "mysql")).IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task TenantService_UsesCoreClaimsTenancyAndNeverTheHeader()
    {
        var files = await GenerateAsync(Manifest(multiTenancy: true));

        var program = Single(files, "/Program.cs");
        program.Should().Contain("AddPervaxisClaimsMultiTenancy").And.Contain("UsePervaxisClaimsTenancy");
        program.Should().Contain("AddPervaxisTenantRepositories");
        program.Should().Contain(".WithTenantExempt()");
        program.IndexOf("UseAuthentication", StringComparison.Ordinal)
            .Should().BeLessThan(program.IndexOf("UsePervaxisClaimsTenancy", StringComparison.Ordinal));
        program.IndexOf("UsePervaxisClaimsTenancy", StringComparison.Ordinal)
            .Should().BeLessThan(program.IndexOf("UseAuthorization", StringComparison.Ordinal));
        program.Should().NotContain("AddDbContextPool");

        files.Keys.Should().NotContain(k => k.EndsWith("/TenantMiddleware.cs", StringComparison.Ordinal));
        files.Keys.Should().NotContain(k => k.Contains("/Infrastructure/Tenancy/", StringComparison.Ordinal));
        files.Values.Should().NotContain(v => v.Contains("X-Tenant-ID", StringComparison.OrdinalIgnoreCase) && !v.Contains("ignored", StringComparison.OrdinalIgnoreCase)
            || v.Contains("TenantMiddleware", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TenantService_CallsTheRawPayloadGuardFromTheTokenValidatedHandler()
    {
        var files = await GenerateAsync(Manifest(multiTenancy: true));

        Single(files, "/Infrastructure/Extensions/AuthenticationExtensions.cs")
            .Should().Contain("TenantClaimGuard.Enforce");
        Single(files, "/Auth/TenantClaimGuard.cs").Should().Contain("TenantClaimPayloadValidator.Validate");
        files.Keys.Should().Contain(k => k.EndsWith("/TenantIsolationTests.cs", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TenantService_DerivesItsDbContextFromTheCoreTenantContext()
    {
        var files = await GenerateAsync(Manifest(multiTenancy: true));

        Single(files, "/Data/BillingServiceDbContext.cs").Should().Contain(": TenantDbContext");
        Single(files, "/Data/DesignTimeDbContextFactory.cs").Should().Contain("DesignTimeTenantContext.Instance");
        Single(files, "/Infrastructure/Middleware/ExceptionHandlerMiddleware.cs")
            .Should().Contain("TenantResourceNotFoundException").And.Contain("DbUpdateConcurrencyException");

        var packages = Single(files, "Directory.Packages.props");
        packages.Should().Contain("Pervaxis.Core.MultiTenancy").And.Contain("Pervaxis.Core.Data.EntityFrameworkCore");

        Single(files, "DeveloperReference/Tenancy.md").Should().Contain("TenantEntity<Guid>").And.Contain("BeginScope");
    }

    [Fact]
    public async Task ServiceWithoutTenancy_IsUnchanged()
    {
        var files = await GenerateAsync(Manifest(multiTenancy: false));

        Single(files, "/Program.cs").Should().NotContain("PervaxisClaims");
        Single(files, "/Data/DesignTimeDbContextFactory.cs").Should().NotContain("DesignTimeTenantContext.Instance");
        files.Keys.Should().Contain(k => k.Contains("/Infrastructure/Tenancy/ITenantContext.cs", StringComparison.Ordinal));
        files.Keys.Should().NotContain(k => k.EndsWith("/TenantClaimGuard.cs", StringComparison.Ordinal));
        files.Keys.Should().NotContain(k => k.EndsWith("/TenantIsolationTests.cs", StringComparison.Ordinal));
        Single(files, "Directory.Packages.props").Should().NotContain("Pervaxis.Core.MultiTenancy");
        files.Keys.Should().NotContain(k => k.EndsWith("DeveloperReference/Tenancy.md", StringComparison.Ordinal));
    }
}
