using System.IO.Compression;
using FluentAssertions;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public class PrintGeneratorTests
{
    private readonly PrintGenerator generator = new();

    [Fact]
    public async Task GenerateAsync_ReturnsZipArchiveForTemplateRoot()
    {
        var manifest = CreateManifest();

        var zipBytes = await generator.GenerateAsync(manifest, "Templates/minimal");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        archive.Entries.Should().ContainSingle();
        archive.GetEntry("hello")!.Should().NotBeNull();
        using var reader = new StreamReader(archive.GetEntry("hello")!.Open());
        var content = await reader.ReadToEndAsync();
        content.Should().Contain("Pervaxis Forge");
    }

    [Fact]
    public async Task GenerateAsync_RejectsInvalidManifest()
    {
        var manifest = CreateManifest() with { ServiceName = "billing-api" };

        Func<Task> action = () => generator.GenerateAsync(manifest, "Templates/rest-api");

        await action.Should().ThrowAsync<InvalidOperationException>();
    }

    private static ForgeManifest CreateManifest() => new()
    {
        Product = "Pervaxis Forge",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "clv",
        CloudProvider = "AWS",
    };
}
