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
        InitialTenantId = TestTenancy.InitialTenantId,
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
    public void Validator_RejectsSingleTenantMode()
    {
        validator.Validate(Manifest(multiTenancy: false, withCognito: false, engine: "mysql")).Errors.Should().Contain(e => e.Contains("Single-tenant"));
    }

    [Fact]
    public async Task TenantService_UsesCoreClaimsTenancyAndNeverTheHeader()
    {
        var files = await GenerateAsync(Manifest(multiTenancy: true));

        var program = Single(files, "/Platform/Hosting/ForgeHost.cs");
        program.Should().Contain("AddPervaxisClaimsMultiTenancy").And.Contain("UsePervaxisClaimsTenancy");
        program.Should().Contain("AddPervaxisTenantRepositories");
        program.Should().Contain(".WithTenantExempt()");
        program.IndexOf("UseAuthentication", StringComparison.Ordinal)
            .Should().BeLessThan(program.IndexOf("UsePervaxisClaimsTenancy", StringComparison.Ordinal));
        program.IndexOf("UsePervaxisClaimsTenancy", StringComparison.Ordinal)
            .Should().BeLessThan(program.IndexOf("UseAuthorization", StringComparison.Ordinal));
        program.Should().NotContain("AddDbContextPool");

        files.Keys.Should().NotContain(k => k.EndsWith("/TenantMiddleware.cs", StringComparison.Ordinal));
        files.Keys.Should().NotContain(k => k.EndsWith("/Platform/Infrastructure/Tenancy/ITenantContext.cs", StringComparison.Ordinal));
        files.Keys.Should().NotContain(k => k.EndsWith("/Platform/Infrastructure/Tenancy/TenantContext.cs", StringComparison.Ordinal));
        files.Values.Should().NotContain(v => v.Contains("X-Tenant-ID", StringComparison.OrdinalIgnoreCase) && !v.Contains("ignored", StringComparison.OrdinalIgnoreCase)
            || v.Contains("TenantMiddleware", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TenantService_CallsTheRawPayloadGuardFromTheTokenValidatedHandler()
    {
        var files = await GenerateAsync(Manifest(multiTenancy: true));

        Single(files, "/Platform/Infrastructure/Extensions/AuthenticationExtensions.cs")
            .Should().Contain("TenantClaimGuard.Enforce");
        Single(files, "/Platform/Auth/TenantClaimGuard.cs").Should().Contain("TenantClaimPayloadValidator.Validate");
        files.Keys.Should().Contain(k => k.EndsWith("/TenantIsolationTests.cs", StringComparison.Ordinal));
    }

    [Fact]
    public async Task TenantService_DerivesItsDbContextFromTheCoreTenantContext()
    {
        var files = await GenerateAsync(Manifest(multiTenancy: true));

        Single(files, "/Platform/Data/BillingServiceDbContext.cs").Should().Contain(": TenantDbContext");
        Single(files, "/Platform/Data/DesignTimeDbContextFactory.cs").Should().Contain("DesignTimeTenantContext.Instance");
        Single(files, "/Platform/Infrastructure/Middleware/ExceptionHandlerMiddleware.cs")
            .Should().Contain("TenantResourceNotFoundException").And.Contain("DbUpdateConcurrencyException");

        var packages = Single(files, "Directory.Packages.props");
        packages.Should().Contain("Pervaxis.Core.MultiTenancy").And.Contain("Pervaxis.Core.Data.EntityFrameworkCore");

        Single(files, "DeveloperReference/Tenancy.md").Should().Contain("TenantEntity<Guid>").And.Contain("BeginScope");
    }

    [Fact]
    public async Task SingleTenantGeneration_IsRejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => GenerateAsync(Manifest(false)));
    }

    [Theory]
    [InlineData(true)]
    public async Task TheFirstTenantId_IsTheSameEverywhereInThePrint(bool multiTenancy)
    {
        var files = await GenerateAsync(Manifest(multiTenancy));

        var defaults = Single(files, "/Platform/Tenancy/Data/TenantDefaults.cs");
        var id = System.Text.RegularExpressions.Regex.Match(defaults, "DefaultTenantId = \"([0-9a-f-]{36})\"").Groups[1].Value;

        Guid.TryParse(id, out var parsed).Should().BeTrue();
        parsed.Should().NotBe(Guid.Empty);
        Single(files, "/appsettings.json").Should().Contain($"\"DefaultTenantId\": \"{id}\"");
        Single(files, "/Data/Migrations/20260101000000_AddTenants.cs").Should().Contain($"new Guid(\"{id}\")");
        Single(files, "/Data/Migrations/BillingServiceDbContextModelSnapshot.cs").Should().Contain($"new Guid(\"{id}\")");
        Single(files, "DeveloperReference/GoingMultiTenant.md").Should().Contain(id);
    }

    [Fact]
    public async Task RelatedPrints_ReuseTheProvidedTenantId()
    {
        var first = Single(await GenerateAsync(Manifest(multiTenancy: true)), "/Platform/Tenancy/Data/TenantDefaults.cs");
        var second = Single(await GenerateAsync(Manifest(multiTenancy: true)), "/Platform/Tenancy/Data/TenantDefaults.cs");

        first.Should().Be(second);
    }

    [Fact]
    public async Task ServiceWithoutDatabase_IsRejected()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() => GenerateAsync(Manifest(true) with { Database = null }));
    }

    [Fact]
    public void InitialTenantId_IsRequiredAndNonEmpty()
    {
        foreach (var id in new Guid?[] { null, Guid.Empty })
            validator.Validate(Manifest(true) with { InitialTenantId = id }).Errors.Should().Contain(e => e.Contains("InitialTenantId"));
    }
}
