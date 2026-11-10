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
using FluentAssertions;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

/// <summary>
/// Generated REST prints keep credentials and personal data inside the service: no localhost CORS origin outside
/// Development, no forwarding of the caller's token to third parties, retries only for repeatable requests,
/// and redaction applied to logs.
/// </summary>
public class OutboundSecurityGenerationTests
{
    private static ForgeManifest Manifest() => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "CLV",
        CloudProvider = "AWS",
        Database = new DatabaseConfig { Engine = "postgresql", Name = "billing" },
    };

    private static async Task<Dictionary<string, string>> GenerateAsync()
    {
        var bytes = await new PrintGenerator().GenerateAsync(Manifest(), "AWS");
        using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name)))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName.Replace('\\', '/')] = reader.ReadToEnd();
        }

        return files;
    }

    private static string Single(Dictionary<string, string> files, string suffix) =>
        files.Single(f => f.Key.EndsWith(suffix, StringComparison.Ordinal)).Value;

    private static string[] AllowedOrigins(string appsettingsJson)
    {
        using var document = JsonDocument.Parse(appsettingsJson);
        return document.RootElement.GetProperty("Forge").GetProperty("Cors").GetProperty("AllowedOrigins")
            .EnumerateArray().Select(e => e.GetString()!).ToArray();
    }

    [Fact]
    public async Task Cors_AllowsLocalhost_OnlyInDevelopment()
    {
        var files = await GenerateAsync();

        AllowedOrigins(Single(files, "/appsettings.json")).Should().BeEmpty();
        AllowedOrigins(Single(files, "/appsettings.Production.json")).Should().BeEmpty();
        AllowedOrigins(Single(files, "/appsettings.Development.json")).Should().Contain("http://localhost:4200");
        Single(files, "/Program.cs").Should().NotContain("?? [\"http://localhost");
    }

    [Fact]
    public async Task ExternalClient_NeverForwardsTheCallersToken()
    {
        var files = await GenerateAsync();
        var handler = Single(files, "/ExternalAuthHandler.cs");

        handler.Should().NotContain("BearerToken").And.NotContain("ForgeAuthenticationOptions");
        handler.Should().Contain("request.Headers.Authorization = null");
        var program = Single(files, "/Program.cs");
        program.Should().Contain("AddOptions<ForgeExternalServicesOptions>()");

        // Redirects would be followed below ExternalAuthHandler and carry the API key to another host.
        var externalClient = program[program.IndexOf("AddHttpClient<IForgeExternalHttpClient", StringComparison.Ordinal)..];
        externalClient = externalClient[..externalClient.IndexOf(".AddForgeResilience(", StringComparison.Ordinal)];
        externalClient.Should().Contain("AllowAutoRedirect = false");
    }

    [Fact]
    public async Task Retries_AreLimitedToRepeatableRequests()
    {
        var files = await GenerateAsync();

        Single(files, "/ResilienceExtensions.cs").Should().Contain("ShouldHandle = args => IsRetrySafe(args.Context.GetRequestMessage())");
        Single(files, "/ResilientHttpClientBuilderExtensions.cs").Should().Contain("!IsRetrySafe(request)");
    }

    [Fact]
    public async Task CircuitBreaker_UsesExplicitRatioAndThroughput()
    {
        var files = await GenerateAsync();
        var resilience = Single(files, "/ResilienceExtensions.cs");

        resilience.Should().NotContain("/ 100.0").And.Contain("MinimumThroughput = minimumThroughput");
        using var appsettings = JsonDocument.Parse(Single(files, "/appsettings.json"));
        var external = appsettings.RootElement.GetProperty("Resilience").GetProperty("External");
        external.GetProperty("CircuitBreakerFailureRatio").GetDouble().Should().Be(0.5);
        external.TryGetProperty("CircuitBreakerFailureThreshold", out _).Should().BeFalse();
    }

    [Fact]
    public async Task OutputCache_RunsAfterAuthorization_AndOnlyWhenEnabled()
    {
        var program = Single(await GenerateAsync(), "/Program.cs");

        var cache = program.IndexOf("app.UseOutputCache()", StringComparison.Ordinal);
        cache.Should().BeGreaterThan(program.IndexOf("app.UseAuthorization()", StringComparison.Ordinal));
        cache.Should().BeGreaterThan(program.IndexOf("app.UseRateLimiter()", StringComparison.Ordinal));
        program.Should().Contain("if (outputCaching.Enabled)");
        program.Should().Contain("options.AddPolicy(ForgeOutputCachingOptions.PublicPolicy");
        program.Split("app.UseOutputCache()").Length.Should().Be(2, "the middleware is registered once");
    }

    [Fact]
    public async Task AuditLog_HonoursEnabled_AndRecordsFailures()
    {
        var program = Single(await GenerateAsync(), "/Program.cs");

        program.Should().Contain("if (auditLogging.Enabled)");
        var audit = program[program.IndexOf("if (auditLogging.Enabled)", StringComparison.Ordinal)..];
        audit.Should().Contain("finally").And.Contain("throw;").And.Contain("{Outcome}");
    }

    [Fact]
    public async Task Logs_AreRedacted()
    {
        var files = await GenerateAsync();

        files.Keys.Should().Contain(k => k.EndsWith("/RedactionEnricher.cs", StringComparison.Ordinal));
        var serilog = Single(files, "/SerilogExtensions.cs");
        // Redaction must run after the enrichers that add properties, including log scopes.
        serilog.IndexOf("Enrich.With(new RedactionEnricher(", StringComparison.Ordinal)
            .Should().BeGreaterThan(serilog.IndexOf("Enrich.FromLogContext()", StringComparison.Ordinal))
            .And.BeGreaterThan(serilog.IndexOf("Enrich.WithSpan()", StringComparison.Ordinal));
        serilog.Should().Contain("loggerConfig.ApplyForgeEnrichment(");
        Single(files, "/HttpLoggingHandler.cs").Should().Contain("WithoutQuery(request.RequestUri)");
        files.Keys.Should().Contain(k => k.EndsWith("/OutboundSecurityTests.cs", StringComparison.Ordinal));
    }
}
