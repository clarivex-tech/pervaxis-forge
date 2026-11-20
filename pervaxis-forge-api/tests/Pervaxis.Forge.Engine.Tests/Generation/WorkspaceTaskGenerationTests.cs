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
using Pervaxis.Forge.Engine.Validation;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

/// <summary>Renders reference modules and proves unchanged source/tests copy into a differently named host.</summary>
public sealed class WorkspaceTaskGenerationTests
{
    private static ForgeManifest Manifest(string name, bool? include = null, bool minimal = false) => new()
    {
        Product = "clarivolt", VerticalSlug = "clarivolt", ServiceName = name + "-service",
        ServiceType = ServiceType.RestApi, ComponentPrefix = "clv", CloudProvider = "AWS",
        MultiTenancy = true, InitialTenantId = TestTenancy.InitialTenantId, Cognito = TestTenancy.Cognito,
        Database = new DatabaseConfig { Engine = "postgresql", Name = "workspace" },
        IncludeWorkspaceTasks = include,
        Observability = minimal ? new ObservabilityConfig
        {
            SerilogEnabled = false, CloudWatchEnabled = false, OpenTelemetryEnabled = false, CorrelationIdEnabled = false,
        } : null,
        Resilience = minimal ? new ResilienceConfig { RetryEnabled = false, CircuitBreakerEnabled = false, TimeoutEnabled = false } : null,
    };

    private static async Task<Dictionary<string, string>> Files(ForgeManifest manifest)
    {
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        var files = new Dictionary<string, string>();
        foreach (var entry in zip.Entries.Where(entry => entry.Name.Length > 0))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName] = await reader.ReadToEndAsync();
        }
        return files;
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DefaultAndOptOut_HaveMatchingReferenceHistory(bool include)
    {
        var files = await Files(Manifest("workspace", include));
        Assert.Equal(include, files.Keys.Any(path => path.EndsWith("/Domains/Workspace/WorkspaceModule.cs", StringComparison.Ordinal)));
        Assert.Equal(include, files.Keys.Any(path => path.EndsWith("/20261120000000_AddWorkspaceTasks.cs", StringComparison.Ordinal)));
        var snapshot = files.Single(file => file.Key.EndsWith("DbContextModelSnapshot.cs", StringComparison.Ordinal)).Value;
        Assert.Equal(include, snapshot.Contains("workspace_tasks", StringComparison.Ordinal));
        foreach (var file in files.Where(file => file.Key.Contains("/Domains/Workspace/", StringComparison.Ordinal)))
            Assert.DoesNotContain("Pervaxis.Clarivolt.Workspace", file.Value);
        Assert.Contains(files.Keys, path => path.EndsWith("/Platform/Contracts/V1/IDomainSession.cs", StringComparison.Ordinal));
        Assert.Contains(files.Keys, path => path.EndsWith("/Platform/Contracts/V1/IUserContext.cs", StringComparison.Ordinal));
    }

    [Fact]
    public void ExplicitlyIncompatibleReferenceSelection_HasAClearError()
    {
        var manifest = Manifest("workspace", true) with { Database = null, Cognito = null };
        Assert.Contains(new ManifestValidator().Validate(manifest).Errors, error => error.Contains("IncludeWorkspaceTasks=true", StringComparison.Ordinal));
        var otherPrint = manifest with { ServiceType = ServiceType.GraphQL };
        Assert.Contains(new ManifestValidator().Validate(otherPrint).Errors, error => error.Contains("IncludeWorkspaceTasks=true", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WorkspaceCopiesUnchanged_WithNoHostWiring_AndEnforcesOwnership(bool minimal)
    {
        var root = Path.Combine(Path.GetTempPath(), "forge-workspace-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var source = await Render(root, "source", Manifest("workspace-source", minimal: minimal));
            var target = await Render(root, "target", Manifest("workspace-target", false, minimal));
            await Run(source, "test", "--verbosity", "minimal");
            await Run(target, "test", "--filter", "FullyQualifiedName~SliceArchitectureTests", "--verbosity", "minimal");
            var before = Directory.GetFiles(Path.Combine(target, "src"), "*.cs", SearchOption.AllDirectories)
                .Where(path => !path.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar, StringComparison.Ordinal))
                .ToDictionary(path => path, File.ReadAllBytes);
            Copy(Path.Combine(Directory.GetDirectories(Path.Combine(source, "src")).Single(), "Domains/Workspace"),
                Path.Combine(Directory.GetDirectories(Path.Combine(target, "src")).Single(), "Domains/Workspace"));
            Copy(Path.Combine(Directory.GetDirectories(Path.Combine(source, "tests")).Single(), "Domains/Workspace"),
                Path.Combine(Directory.GetDirectories(Path.Combine(target, "tests")).Single(), "Domains/Workspace"));
            foreach (var pair in before) Assert.Equal(pair.Value, await File.ReadAllBytesAsync(pair.Key));
            // New schema is intentional; existing application history is preserved rather than overwritten.
            await Run(target, "test", "--filter", "FullyQualifiedName!~PlatformModelMatchesPreservedMigrationSnapshot", "--verbosity", "minimal");
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    [Fact]
    public async Task ReferenceValidation_RemainsSafeWhenGlobalValidationIsDisabled()
    {
        var root = Path.Combine(Path.GetTempPath(), "forge-workspace-validation-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            var source = await Render(root, "source", Manifest("workspace-validation") with
            {
                Validation = new ValidationConfig { FluentValidationEnabled = false },
            });
            await Run(source, "test", "--filter", "FullyQualifiedName~Pervaxis.Forge.Reference.Domains.Workspace.Tests", "--verbosity", "minimal");
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static async Task<string> Render(string root, string directory, ForgeManifest manifest)
    {
        var path = Path.Combine(root, directory);
        Directory.CreateDirectory(path);
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        zip.ExtractToDirectory(path);
        return path;
    }

    private static void Copy(string from, string to)
    {
        foreach (var file in Directory.GetFiles(from, "*", SearchOption.AllDirectories))
        {
            var destination = Path.Combine(to, Path.GetRelativePath(from, file));
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            File.Copy(file, destination);
            Assert.Equal(File.ReadAllBytes(file), File.ReadAllBytes(destination));
        }
    }

    private static async Task Run(string directory, params string[] arguments)
    {
        var start = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = directory, RedirectStandardOutput = true, RedirectStandardError = true,
            UseShellExecute = false, CreateNoWindow = true,
        };
        foreach (var argument in arguments) start.ArgumentList.Add(argument);
        using var process = Process.Start(start)!;
        var output = process.StandardOutput.ReadToEndAsync();
        var error = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(entireProcessTree: true); throw; }
        Assert.True(process.ExitCode == 0, await output + "\n" + await error);
    }
}
