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
using System.Text.Json;
using System.Text.RegularExpressions;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

/// <summary>Checks startup instructions against real generated projects and host configuration.</summary>
public sealed class LocalStartupDocumentationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GuideMatchesGeneratedProjectAndConfiguration(bool minimal)
    {
        var manifest = new ForgeManifest
        {
            Product = "example", VerticalSlug = "example", ServiceName = "local-start-service",
            ComponentPrefix = "exm", CloudProvider = "AWS", ServiceType = ServiceType.RestApi,
            InitialTenantId = TestTenancy.InitialTenantId, MultiTenancy = true,
            Cognito = TestTenancy.Cognito,
            Database = new DatabaseConfig { Engine = "postgresql", Name = "example" },
            Observability = minimal ? new ObservabilityConfig
            {
                SerilogEnabled = false, CloudWatchEnabled = false, OpenTelemetryEnabled = false, CorrelationIdEnabled = false,
            } : null,
            Resilience = minimal ? new ResilienceConfig { RetryEnabled = false, CircuitBreakerEnabled = false, TimeoutEnabled = false } : null,
        };
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        var files = new Dictionary<string, string>();
        foreach (var entry in zip.Entries.Where(entry => entry.Name.Length > 0))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName] = await reader.ReadToEndAsync();
        }
        const string guidePath = "DeveloperReference/LocalDevelopment.md";
        var guide = files[guidePath];
        var project = files.Keys.Single(path => path.StartsWith("src/", StringComparison.Ordinal) && path.EndsWith(".csproj", StringComparison.Ordinal));
        Assert.Contains("dotnet run --project './" + project + "' --no-launch-profile --urls 'http://localhost:5080'", guide);
        Assert.Contains("dotnet build './" + project + "' --no-restore", guide);
        Assert.Contains("DeveloperReference/LocalDevelopment.md", files["README.md"]);
        Assert.Contains("LocalDevelopment.md", files["DeveloperReference/README.md"]);
        Assert.DoesNotContain("{{", guide);
        Assert.DoesNotContain("$env:Forge__LocalMode =", guide);
        Assert.DoesNotContain("dotnet ef database update", files["README.md"]);
        Assert.Contains("$env:DOTNET_ENVIRONMENT = 'Development'", guide);
        Assert.Contains("$env:ASPNETCORE_ENVIRONMENT = 'Development'", guide);
        Assert.Contains("$env:AWS_PROFILE = '<development-profile>'", guide);
        Assert.Contains("$env:AWS_REGION = '<aws-region>'", guide);
        Assert.Contains("$env:AWS__Region = $env:AWS_REGION", guide);
        Assert.Contains("http://localhost:5080/health/ready", guide);
        Assert.Contains("apiUrl", guide);
        Assert.Contains("http://localhost:4200", guide);
        Assert.Contains("migration credentials", guide);
        foreach (Match link in Regex.Matches(guide, @"\]\(([^)#]+\.md)\)"))
        {
            var target = "DeveloperReference/" + link.Groups[1].Value;
            Assert.True(files.ContainsKey(target), "Broken local guide link: " + target);
        }
        var settingsPath = project[..project.LastIndexOf('/')];
        using var settings = JsonDocument.Parse(files[settingsPath + "/appsettings.json"]);
        using var development = JsonDocument.Parse(files[settingsPath + "/appsettings.Development.json"]);
        var forge = settings.RootElement.GetProperty("Forge");
        Assert.Equal("http://localhost:4200", development.RootElement.GetProperty("Forge").GetProperty("Cors").GetProperty("AllowedOrigins")[0].GetString());
        var host = files.Single(file => file.Key.EndsWith("/Platform/Hosting/ForgeHost.cs", StringComparison.Ordinal)).Value;
        Assert.Contains("Forge:Cors:AllowedOrigins", host);
        Assert.Contains("GetAWSOptions()", host);
        Assert.Contains("$env:Forge__Database__SecretId", guide);
        Assert.Contains("$env:Forge__Database__RootCertificate", guide);
        Assert.Contains("SSL Mode=VerifyFull", guide);
        Assert.Contains("Root Certificate=<ca-path>", guide);
        Assert.Contains("Never replace history", guide);
        Assert.True(forge.GetProperty("Database").TryGetProperty("SecretId", out _));
        Assert.True(forge.GetProperty("Database").TryGetProperty("RootCertificate", out _));
        Assert.True(settings.RootElement.GetProperty("ConnectionStrings").TryGetProperty("Default", out _));
        var factory = files.Single(file => file.Key.EndsWith("/DesignTimeDbContextFactory.cs", StringComparison.Ordinal)).Value;
        Assert.Contains("AddEnvironmentVariables()", factory);
        Assert.DoesNotContain("GetSecretValue", factory);
        Assert.Contains("$env:Forge__Provisioning__Enabled = 'false'", guide);
        Assert.False(forge.GetProperty("Provisioning").GetProperty("Enabled").GetBoolean());
        Assert.Contains("cognito-idp:AdminCreateUser", guide);
        Assert.Contains("must match token-validation Authority", guide);
        await BuildDocumentedProject(zip, project);
    }

    private static async Task BuildDocumentedProject(ZipArchive zip, string project)
    {
        var root = Path.Combine(Path.GetTempPath(), "forge-local-docs-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(root);
        try
        {
            zip.ExtractToDirectory(root);
            await RunDotnet(root, "restore", "./" + project);
            await RunDotnet(root, "build", "./" + project, "--no-restore");
        }
        finally { Directory.Delete(root, recursive: true); }
    }

    private static async Task RunDotnet(string directory, params string[] arguments)
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
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(3));
        try { await process.WaitForExitAsync(timeout.Token); }
        catch (OperationCanceledException) { process.Kill(entireProcessTree: true); throw; }
        Assert.True(process.ExitCode == 0, await output + "\n" + await error);
    }
}
