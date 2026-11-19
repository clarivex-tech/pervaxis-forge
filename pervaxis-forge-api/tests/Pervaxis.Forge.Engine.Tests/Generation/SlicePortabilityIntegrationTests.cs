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

using System.Diagnostics;
using System.IO.Compression;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

/// <summary>Compiles real prints and exercises copied source through production discovery, without AWS or PostgreSQL.</summary>
public sealed class SlicePortabilityIntegrationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CopyToDifferentlyNamedFreshPrintRequiresNoHostEdits(bool minimalInfrastructure)
    {
        var root = Path.Combine(Path.GetTempPath(), "forge-port-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var source = await GenerateAsync(root, "source", minimalInfrastructure);
            var target = await GenerateAsync(root, "target", minimalInfrastructure);
            // First prove that regrouping platform files has not changed the PostgreSQL model/snapshot.
            await RunAsync(target, "test", "--filter", "FullyQualifiedName~SliceArchitectureTests", "--verbosity", "minimal");

            var sourceFeature = Path.Combine(source, "src/Pervaxis.Clarivolt.Source/Domains/Catalog");
            Directory.CreateDirectory(sourceFeature);
            foreach (var (name, content) in FeatureFiles())
                await File.WriteAllTextAsync(Path.Combine(sourceFeature, name), License + content);
            var before = Directory.GetFiles(Path.Combine(target, "src"), "*.cs", SearchOption.AllDirectories)
                .ToDictionary(path => path, File.ReadAllBytes);
            var targetFeature = Path.Combine(target, "src/Pervaxis.Clarivolt.Target/Domains/Catalog");
            Directory.CreateDirectory(targetFeature);
            foreach (var path in Directory.GetFiles(sourceFeature))
            {
                var destination = Path.Combine(targetFeature, Path.GetFileName(path));
                File.Copy(path, destination);
                Assert.Equal(await File.ReadAllBytesAsync(path), await File.ReadAllBytesAsync(destination));
            }
            foreach (var (path, content) in before)
                Assert.Equal(content, await File.ReadAllBytesAsync(path));
            Directory.CreateDirectory(Path.Combine(target, "tests/Pervaxis.Clarivolt.Target.Tests/Domains/Catalog"));
            await File.WriteAllTextAsync(Path.Combine(target, "tests/Pervaxis.Clarivolt.Target.Tests/Domains/Catalog/CopiedCatalogTests.cs"), License + RuntimeTest);
            // New feature intentionally needs an appended migration; it must not match the starter snapshot.
            await RunAsync(target, "test", "--filter", "FullyQualifiedName!~PlatformModelMatchesPreservedMigrationSnapshot", "--verbosity", "minimal");

            // Mutation checks: the architecture rules must actually catch runtime coupling.
            var forbidden = Path.Combine(target, "src/Pervaxis.Clarivolt.Target/Platform/ForbiddenHostDependency.cs");
            await File.WriteAllTextAsync(forbidden, License + """
                namespace Host;
                public sealed class ForbiddenHostDependency(Example.Domains.Catalog.CatalogService service)
                {
                    public object Service => service;
                }
                """);
            var failure = await RunProcessAsync(target, false, "test", "--no-restore", "--filter",
                "FullyQualifiedName~RuntimeHostDoesNotDependOnBusinessSlices", "--verbosity", "minimal");
            Assert.Contains("ForbiddenHostDependency", failure);
            File.Delete(forbidden);
            await File.WriteAllTextAsync(Path.Combine(targetFeature, "ForbiddenSliceDependency.cs"), License + """
                namespace Example.Domains.Other;
                public sealed class ForbiddenSliceDependency(Example.Domains.Catalog.CatalogService service)
                {
                    public object Service => service;
                }
                """);
            failure = await RunProcessAsync(target, false, "test", "--no-restore", "--filter",
                "FullyQualifiedName~RuntimeHostDoesNotDependOnBusinessSlices", "--verbosity", "minimal");
            Assert.Contains("ForbiddenSliceDependency", failure);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<string> GenerateAsync(string root, string name, bool minimalInfrastructure)
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt", VerticalSlug = "clarivolt", ServiceName = name + "-service",
            ServiceType = ServiceType.RestApi, ComponentPrefix = "clv", CloudProvider = "AWS",
            MultiTenancy = true, InitialTenantId = TestTenancy.InitialTenantId, Cognito = TestTenancy.Cognito,
            Database = new DatabaseConfig { Engine = "postgresql", Name = "portability" },
            Observability = minimalInfrastructure ? new ObservabilityConfig
            {
                SerilogEnabled = false, CloudWatchEnabled = false,
                OpenTelemetryEnabled = false, CorrelationIdEnabled = false,
            } : null,
            Resilience = minimalInfrastructure ? new ResilienceConfig
            {
                RetryEnabled = false, CircuitBreakerEnabled = false, TimeoutEnabled = false,
            } : null,
        };
        var directory = Path.Combine(root, name);
        Directory.CreateDirectory(directory);
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        zip.ExtractToDirectory(directory);
        return directory;
    }

    private static async Task RunAsync(string directory, params string[] arguments)
        => await RunProcessAsync(directory, true, arguments);

    private static async Task<string> RunProcessAsync(string directory, bool expectSuccess, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory, RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false, CreateNoWindow = true,
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(entireProcessTree: true); throw; }
        var result = await output + "\n" + await error;
        Assert.True((process.ExitCode == 0) == expectSuccess, result);
        return result;
    }

    private static Dictionary<string, string> FeatureFiles() => new()
    {
        ["CatalogModule.cs"] = """
            using Microsoft.Extensions.Configuration;
            using Microsoft.Extensions.DependencyInjection;
            using Pervaxis.Forge.Contracts.V1;
            namespace Example.Domains.Catalog;
            public sealed class CatalogModule : IDomainModule
            {
                public void Register(IServiceCollection services, IConfiguration configuration)
                    => services.AddScoped<CatalogService>();
            }
            """,
        ["CatalogItem.cs"] = """
            using Pervaxis.Core.Data.EntityFrameworkCore;
            namespace Example.Domains.Catalog;
            public sealed class CatalogItem : TenantEntity<Guid>
            {
                public CatalogItem(Guid id, string name) { Id = id; Name = name; }
                public string Name { get; set; }
            }
            """,
        ["CatalogConfiguration.cs"] = """
            using Microsoft.EntityFrameworkCore;
            using Microsoft.EntityFrameworkCore.Metadata.Builders;
            namespace Example.Domains.Catalog;
            public sealed class CatalogConfiguration : IEntityTypeConfiguration<CatalogItem>
            {
                public void Configure(EntityTypeBuilder<CatalogItem> builder)
                {
                    builder.ToTable("catalog_items");
                    builder.Property(item => item.Name).IsRequired().HasMaxLength(80);
                }
            }
            """,
        ["CreateItemRequest.cs"] = """
            namespace Example.Domains.Catalog;
            public sealed record CreateItemRequest(string Name);
            """,
        ["CreateItemValidator.cs"] = """
            using FluentValidation;
            namespace Example.Domains.Catalog;
            public sealed class CreateItemValidator : AbstractValidator<CreateItemRequest>
            {
                public CreateItemValidator() => RuleFor(request => request.Name).NotEmpty().MaximumLength(80);
            }
            """,
        ["CatalogService.cs"] = """
            using Microsoft.EntityFrameworkCore;
            using Pervaxis.Core.Abstractions.Data;
            using Pervaxis.Forge.Contracts.V1;
            namespace Example.Domains.Catalog;
            public sealed class CatalogService(IRepository<CatalogItem, Guid> items, IUnitOfWork work, IDomainQueries queries)
            {
                public Task<CatalogItem?> GetAsync(Guid id, CancellationToken ct)
                    => queries.Query<CatalogItem>().SingleOrDefaultAsync(item => item.Id == id, ct);
                public async Task<Guid> CreateAsync(string name, CancellationToken ct)
                {
                    var item = new CatalogItem(Guid.NewGuid(), name);
                    await items.AddAsync(item, ct);
                    await work.CommitAsync(ct);
                    return item.Id;
                }
            }
            """,
        ["CatalogController.cs"] = """
            using Microsoft.AspNetCore.Mvc;
            namespace Example.Domains.Catalog;
            [ApiController]
            [Route("api/v1/catalog")]
            public sealed class CatalogController(CatalogService service) : ControllerBase
            {
                [HttpPost]
                public async Task<IActionResult> Create(CreateItemRequest request, CancellationToken ct)
                    => Ok(await service.CreateAsync(request.Name, ct));
                [HttpGet("{id:guid}")]
                public async Task<IActionResult> Get(Guid id, CancellationToken ct)
                {
                    var item = await service.GetAsync(id, ct);
                    return item is null ? NotFound() : Ok(item.Name);
                }
            }
            """,
    };

    private const string License = """
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

        """;

    private const string RuntimeTest = """
        using System.Net;
        using System.Net.Http.Json;
        using Example.Domains.Catalog;
        using FluentValidation;
        using Microsoft.AspNetCore.Builder;
        using Microsoft.AspNetCore.Hosting;
        using Microsoft.AspNetCore.TestHost;
        using Microsoft.EntityFrameworkCore;
        using Microsoft.Extensions.DependencyInjection;
        using Pervaxis.Core.Abstractions.MultiTenancy;
        using Pervaxis.Core.Data.EntityFrameworkCore;
        using Pervaxis.Core.MultiTenancy;
        using Pervaxis.Clarivolt.Target.Platform.Data;
        using Pervaxis.Clarivolt.Target.Platform.Extensions;
        using Pervaxis.Clarivolt.Target.Platform.Infrastructure.Domains;
        using Pervaxis.Clarivolt.Target.Platform.Infrastructure.Extensions;
        using Pervaxis.Clarivolt.Target.Platform.Infrastructure.Middleware;
        using Xunit;

        using Pervaxis.Clarivolt.Target.Tests.Platform.Fixtures;

        namespace Example.Domains.Catalog.Tests;
        public sealed class CopiedCatalogTests
        {
            [Fact]
            public async Task CopiedFeatureIsDiscoveredAndIsolated()
            {
                using var database = new TenantTestDatabase();
                var builder = WebApplication.CreateBuilder();
                builder.WebHost.UseTestServer();
                builder.Services.AddTargetServiceServices(builder.Configuration);
                var count = builder.Services.Count;
                builder.Services.AddForgeDomains(builder.Configuration);
                Assert.Equal(count, builder.Services.Count);
                builder.Services.AddControllers().AddApplicationPart(typeof(Pervaxis.Clarivolt.Target.Platform.Hosting.ForgeHost).Assembly);
                builder.Services.AddForgeValidation();
                builder.Services.AddPervaxisClaimsMultiTenancy(options =>
                {
                    options.Mode = TenancyMode.MultiTenant;
                    options.EnableDirectoryChecks = false;
                });
                builder.Services.AddScoped(provider => new TargetServiceDbContext(database.Options, provider.GetRequiredService<ITenantContext>()));
                builder.Services.AddPervaxisTenantRepositories<TargetServiceDbContext>();
                await using var app = builder.Build();
                app.UseMiddleware<ExceptionHandlerMiddleware>();
                // Test-only tenant scope, no authentication provider or external service is called.
                app.Use(async (context, next) =>
                {
                    using var tenant = context.RequestServices.GetRequiredService<ITenantScopeFactory>()
                        .BeginScope(new TenantId(context.Request.Headers["Test-Tenant"].ToString()));
                    await next(context);
                });
                app.MapControllers();
                await app.StartAsync();
                await using var scope = app.Services.CreateAsyncScope();
                Assert.NotNull(scope.ServiceProvider.GetRequiredService<CatalogService>());
                Assert.NotNull(scope.ServiceProvider.GetRequiredService<IValidator<CreateItemRequest>>());
                var db = scope.ServiceProvider.GetRequiredService<TargetServiceDbContext>();
                var entity = db.Model.FindEntityType(typeof(CatalogItem))!;
                Assert.Equal("catalog_items", entity.GetTableName());
                Assert.Equal(80, entity.FindProperty("Name")!.GetMaxLength());
                Assert.True(db.Database.HasPendingModelChanges(), "The copied feature requires an appended migration.");
                using var client = app.GetTestClient();
                var tenantA = Guid.NewGuid().ToString();
                client.DefaultRequestHeaders.Add("Test-Tenant", tenantA);
                Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/v1/catalog", new { Name = "" })).StatusCode);
                var response = await client.PostAsJsonAsync("/api/v1/catalog", new { Name = "Portable" });
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                var id = await response.Content.ReadFromJsonAsync<Guid>();
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/catalog/{id}")).StatusCode);
                client.DefaultRequestHeaders.Remove("Test-Tenant");
                client.DefaultRequestHeaders.Add("Test-Tenant", Guid.NewGuid().ToString());
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/catalog/{id}")).StatusCode);
                var second = await client.PostAsJsonAsync("/api/v1/catalog", new { Name = "Other tenant" });
                Assert.Equal(HttpStatusCode.OK, second.StatusCode);
                var otherId = await second.Content.ReadFromJsonAsync<Guid>();
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync($"/api/v1/catalog/{otherId}")).StatusCode);
                client.DefaultRequestHeaders.Remove("Test-Tenant");
                client.DefaultRequestHeaders.Add("Test-Tenant", tenantA);
                Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/v1/catalog/{otherId}")).StatusCode);
            }
        }
        """;
}
