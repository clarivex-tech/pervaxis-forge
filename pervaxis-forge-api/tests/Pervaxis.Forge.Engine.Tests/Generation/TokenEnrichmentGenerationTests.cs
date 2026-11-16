// Copyright (C) 2026 Clarivex Technologies Private Limited. All Rights Reserved.

using System.IO.Compression;
using System.Text.Json;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Validation;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public sealed class TokenEnrichmentGenerationTests
{
    private static ForgeManifest Manifest() => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "identity-token-enrichment",
        ComponentPrefix = "clv",
        ServiceType = ServiceType.IdentityTokenEnrichment,
        CloudProvider = "AWS",
        TokenEnrichment = new TokenEnrichmentConfig { Region = "us-east-1", UserPoolId = "us-east-1_Example" }
    };

    [Fact]
    public async Task GeneratesStandaloneConsoleDeployablePrintWithProtectedPoolPermission()
    {
        var generator = new PrintGenerator();
        var zip = await generator.GenerateAsync(Manifest(), "AWS");
        using var archive = new ZipArchive(new MemoryStream(zip));
        Assert.Equal(5, archive.Entries.Count);
        foreach (var path in new[] { "template.yaml", "README.md", "samconfig.toml", "tests/test_handler.py", "events/sign-in.json" })
            Assert.NotNull(archive.GetEntry(path));
        using var reader = new StreamReader(archive.GetEntry("template.yaml")!.Open());
        var template = await reader.ReadToEndAsync();
        Assert.Contains("Runtime: python3.13", template);
        Assert.Contains("InlineCode: |", template);
        Assert.Contains("Handler: index.handler", template);
        Assert.Contains("Default: 'us-east-1_Example'", template);
        Assert.Contains("SourceAccount: !Ref AWS::AccountId", template);
        Assert.Contains("userpool/${UserPoolId}", template);
        Assert.DoesNotContain("AWS::Cognito::UserPool", template);
        Assert.DoesNotContain("Pervaxis.Genesis", template);
        Assert.DoesNotContain("{{", template);
        using var eventReader = new StreamReader(archive.GetEntry("events/sign-in.json")!.Open());
        using var example = JsonDocument.Parse(await eventReader.ReadToEndAsync());
        Assert.Equal("us-east-1_Example", example.RootElement.GetProperty("userPoolId").GetString());
    }

    [Theory]
    [InlineData("", "us-east-1_Example")]
    [InlineData("us-east-1", "")]
    [InlineData("us-east-1", "eu-west-1_Example")]
    [InlineData("us-east-1", "us-east-1_Example'\nInjected: true")]
    [InlineData("bad-region", "bad-region_Example")]
    public void RejectsMissingMismatchedOrUnsafeDeploymentSettings(string region, string pool)
    {
        var manifest = Manifest() with { TokenEnrichment = new TokenEnrichmentConfig { Region = region, UserPoolId = pool } };
        Assert.False(new ManifestValidator().Validate(manifest).IsValid);
    }

    [Fact]
    public async Task RejectsUnsupportedCloudAndRequiresSettings()
    {
        var validator = new ManifestValidator();
        Assert.False(validator.Validate(Manifest() with { CloudProvider = "Azure" }).IsValid);
        Assert.False(validator.Validate(Manifest() with { TokenEnrichment = null }).IsValid);
        await Assert.ThrowsAsync<InvalidOperationException>(() => new PrintGenerator().GenerateAsync(Manifest(), "GCP"));
    }
}
