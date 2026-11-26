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
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

/// <summary>
/// Production observability of the generated REST API print. The print must never ship a collector address,
/// must refuse to start without one outside Development, and must give support a trace ID for every failure.
/// </summary>
public class ObservabilityGenerationTests
{
    private readonly PrintGenerator generator = new();

    private static ForgeManifest RestApiManifest() => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "CLV",
        CloudProvider = "AWS",
        Database = new DatabaseConfig { Engine = "postgresql", Name = "billing" },
        MultiTenancy = true,
        InitialTenantId = TestTenancy.InitialTenantId,
        Cognito = new CognitoConfig
        {
            UserPoolId = "us-east-1_AbC123xyz",
            ClientIds = ["1h57kf5cpq17m0eml12EXAMPLE"],
        },
    };

    private async Task<Dictionary<string, string>> GenerateAsync()
    {
        var zipBytes = await generator.GenerateAsync(RestApiManifest(), "AWS");
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
    public async Task TheBaseSettings_CarryNoCollectorAddress()
    {
        var files = await GenerateAsync();

        Single(files, "/appsettings.json").Should().Contain("\"OtlpEndpoint\": \"\"").And.NotContain("4317");
    }

    [Fact]
    public async Task TheDevelopmentSettings_CarryNoCollectorAddress()
    {
        var files = await GenerateAsync();

        Single(files, "/appsettings.Development.json").Should().NotContain("4317");
    }

    [Fact]
    public async Task TheTracing_RefusesToStartWithoutAnEndpointOutsideDevelopment()
    {
        var files = await GenerateAsync();

        Single(files, "/TracingExtensions.cs").Should()
            .Contain("OpenTelemetry:OtlpEndpoint is required outside Development");
    }

    [Fact]
    public async Task TheTracing_RegistersTheServiceSourceAndMeter()
    {
        var files = await GenerateAsync();

        var tracing = Single(files, "/TracingExtensions.cs");
        tracing.Should().Contain("AddSource(ForgeTelemetry.ActivitySourceName)");
        tracing.Should().Contain("AddMeter(ForgeTelemetry.MeterName)");
    }

    [Fact]
    public async Task TheLogs_AreWrittenToStdoutInEveryEnvironment()
    {
        var files = await GenerateAsync();

        var serilog = Single(files, "/SerilogExtensions.cs");
        serilog.Should().Contain("loggerConfig.WriteTo.Console(new RenderedCompactJsonFormatter());");
        serilog.Should().Contain("!context.HostingEnvironment.IsDevelopment() && options.CloudWatch is { Enabled: true }");
    }

    [Fact]
    public async Task TheCloudWatchSink_CanBeSwitchedOffPerEnvironment()
    {
        var files = await GenerateAsync();

        Single(files, "/ForgeLoggingOptions.cs").Should().Contain("public bool Enabled { get; set; } = true;");
        Single(files, "/appsettings.json").Should().Contain("\"Enabled\": true");
    }

    [Fact]
    public async Task TheErrorResponse_CarriesTheTraceId()
    {
        var files = await GenerateAsync();

        Single(files, "/ExceptionHandlerMiddleware.cs").Should().Contain("traceId");
    }

    [Fact]
    public async Task TheAuditLog_RecordsRouteTenantAndTrace()
    {
        var files = await GenerateAsync();

        Single(files, "/RequestAuditMiddleware.cs").Should()
            .Contain("{Route}").And.Contain("{TenantId}").And.Contain("{TraceId}");
    }

    [Fact]
    public async Task TheHealthProbes_AreExcludedFromTheAuditByDefault()
    {
        var files = await GenerateAsync();

        Single(files, "/ForgeObservabilityOptions.cs").Should()
            .Contain("\"/health\"").And.Contain("\"/health/ready\"");
    }

    [Fact]
    public async Task TheHost_UsesTheAuditMiddleware_AndPassesThePlatformEnvironmentToTracing()
    {
        var files = await GenerateAsync();

        var host = Single(files, "/ForgeHost.cs");
        host.Should().Contain("app.UseMiddleware<RequestAuditMiddleware>()");
        host.Should().Contain("AddForgeTracing(builder.Configuration, builder.Environment)");
    }

    [Fact]
    public async Task TheObservabilityReference_NamesEverySettingEnvironmentsMustProvide()
    {
        var files = await GenerateAsync();

        Single(files, "DeveloperReference/Observability.md").Should()
            .Contain("OpenTelemetry__OtlpEndpoint")
            .And.Contain("Forge__Observability__SlowRequestThresholdMilliseconds")
            .And.Contain("Serilog__CloudWatch__Enabled")
            .And.Contain("Serilog__MinimumLevel__Default")
            .And.Contain("retention");
    }
}
