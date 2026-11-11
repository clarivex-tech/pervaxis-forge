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
/// Switching an infrastructure option off removes its files and packages. Nothing else in the print may still
/// reference them, or the print does not compile.
/// </summary>
public class InfrastructureToggleGenerationTests
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

    /// <summary>The generated service code (src only), file path to content.</summary>
    private static async Task<Dictionary<string, string>> GenerateSourceAsync(ForgeManifest manifest)
    {
        var bytes = await new PrintGenerator().GenerateAsync(manifest, "AWS");
        using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in archive.Entries.Where(e => e.FullName.Replace('\\', '/').Contains("/src/", StringComparison.Ordinal)
                     && (e.Name.EndsWith(".cs", StringComparison.Ordinal) || e.Name.EndsWith(".csproj", StringComparison.Ordinal))))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName.Replace('\\', '/')] = reader.ReadToEnd();
        }

        return files;
    }

    private static void ShouldNotReference(Dictionary<string, string> files, string text) =>
        files.Where(f => f.Value.Contains(text, StringComparison.Ordinal)).Select(f => f.Key)
            .Should().BeEmpty($"'{text}' is left out of the print when its option is off");

    [Fact]
    public async Task SerilogOff_LeavesNoReferenceToTheLoggingHandler()
    {
        var files = await GenerateSourceAsync(Manifest() with
        {
            Observability = new ObservabilityConfig { SerilogEnabled = false, CloudWatchEnabled = false },
        });

        ShouldNotReference(files, "HttpLoggingHandler");
        ShouldNotReference(files, "using Serilog");
    }

    [Fact]
    public async Task CorrelationIdOff_LeavesNoReferenceToTheCorrelationHandlers()
    {
        var files = await GenerateSourceAsync(Manifest() with
        {
            Observability = new ObservabilityConfig { CorrelationIdEnabled = false },
        });

        ShouldNotReference(files, "CorrelationIdHandler");
        ShouldNotReference(files, "CorrelationIdMiddleware");
    }

    [Fact]
    public async Task FluentValidationOff_LeavesNoReferenceToFluentValidation()
    {
        var files = await GenerateSourceAsync(Manifest() with
        {
            Validation = new ValidationConfig { FluentValidationEnabled = false },
        });

        ShouldNotReference(files, "using FluentValidation");
        ShouldNotReference(files, "FluentValidation.");
    }

    [Fact]
    public async Task ResilienceOff_LeavesNoReferenceToPollyOrTheResiliencePipeline()
    {
        var files = await GenerateSourceAsync(Manifest() with
        {
            Resilience = new ResilienceConfig { RetryEnabled = false, CircuitBreakerEnabled = false, TimeoutEnabled = false },
        });

        ShouldNotReference(files, "using Polly");
        ShouldNotReference(files, "BrokenCircuitException");
        ShouldNotReference(files, ".AddForgeResilience(builder.Configuration");
        ShouldNotReference(files, ".AddForgeResilience(configuration");
    }
}
