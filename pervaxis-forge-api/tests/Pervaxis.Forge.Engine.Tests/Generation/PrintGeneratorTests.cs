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

    [Fact]
    public async Task GenerateAsync_IntakeServiceZip_ContainsAllExpectedFiles()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "intake-service",
            ServiceType = ServiceType.RestApi,
            ComponentPrefix = "clv",
            CloudProvider = "AWS",
            GenesisModules = ["FileStorage", "Messaging"],
            Database = new DatabaseConfig { Engine = "postgresql", Name = "intake_db" },
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        var entries = archive.Entries.Select(e => e.FullName).ToList();

        entries.Should().Contain("manifest.json");
        entries.Should().Contain("Program.cs");
        entries.Should().Contain("ServiceCollectionExtensions.cs");
        entries.Should().Contain("Controller.cs");
        entries.Should().Contain("IService.cs");
        entries.Should().Contain("Request.cs");
        entries.Should().Contain("Response.cs");
        entries.Should().Contain("Dockerfile");
        entries.Should().Contain("appsettings.json");
        entries.Should().Contain("appsettings.Development.json");

        // Verify cloud provider flows into Program.cs content
        using var programStream = new StreamReader(archive.GetEntry("Program.cs")!.Open());
        var programContent = await programStream.ReadToEndAsync();
        programContent.Should().Contain("AddGenesisFileStorageAWS");
        programContent.Should().Contain("AddGenesisMessagingAWS");
        programContent.Should().Contain("Pervaxis.Genesis.FileStorage.AWS");

        // Verify csproj has Genesis package references
        using var csprojStream = new StreamReader(archive.GetEntry("csproj")!.Open());
        var csprojContent = await csprojStream.ReadToEndAsync();
        csprojContent.Should().Contain("Pervaxis.Genesis.FileStorage.AWS");
        csprojContent.Should().Contain("Npgsql.EntityFrameworkCore.PostgreSQL");
    }

    private static ForgeManifest CreateManifest() => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "clv",
        CloudProvider = "AWS",
    };
}
