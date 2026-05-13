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
    public async Task GenerateAsync_ReturnsNonEmptyZipForRestApiManifest()
    {
        var manifest = CreateManifest();

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        archive.Entries.Should().NotBeEmpty();
        archive.Entries.Should().Contain(e => e.FullName == "manifest.json");
    }

    [Fact]
    public async Task GenerateAsync_RejectsInvalidManifest()
    {
        var manifest = CreateManifest() with { ServiceName = "billing-api" };

        Func<Task> action = () => generator.GenerateAsync(manifest, "AWS");

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
