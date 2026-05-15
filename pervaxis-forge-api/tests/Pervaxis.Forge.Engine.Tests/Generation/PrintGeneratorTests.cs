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

        const string src = "src/Pervaxis.Clarivolt.Intake";
        const string tests = "tests/Pervaxis.Clarivolt.Intake.Tests";

        entries.Should().Contain("manifest.json");
        entries.Should().Contain($"{src}/Program.cs");
        entries.Should().Contain($"{src}/Extensions/ServiceCollectionExtensions.cs");
        entries.Should().Contain($"{src}/Controllers/Controller.cs");
        entries.Should().Contain($"{src}/Services/IService.cs");
        entries.Should().Contain($"{src}/Models/Request.cs");
        entries.Should().Contain($"{src}/Models/Response.cs");
        entries.Should().Contain("Dockerfile");
        entries.Should().Contain($"{src}/appsettings.json");
        entries.Should().Contain($"{src}/appsettings.Development.json");
        entries.Should().Contain($"{tests}/TestBase.cs");

        // Verify cloud provider flows into Program.cs content
        using var programStream = new StreamReader(archive.GetEntry($"{src}/Program.cs")!.Open());
        var programContent = await programStream.ReadToEndAsync();
        programContent.Should().Contain("AddGenesisFileStorageAWS");
        programContent.Should().Contain("AddGenesisMessagingAWS");
        programContent.Should().Contain("Pervaxis.Genesis.FileStorage.AWS");

        // Verify csproj has Genesis package references and correct name
        entries.Should().Contain($"{src}/Pervaxis.Clarivolt.Intake.csproj");
        using var csprojStream = new StreamReader(archive.GetEntry($"{src}/Pervaxis.Clarivolt.Intake.csproj")!.Open());
        var csprojContent = await csprojStream.ReadToEndAsync();
        csprojContent.Should().Contain("Pervaxis.Genesis.FileStorage.AWS");
        csprojContent.Should().Contain("Npgsql.EntityFrameworkCore.PostgreSQL");
    }

    [Fact]
    public async Task GenerateAsync_IntakeServiceZip_ExtractsAllFilesToDisk()
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

        var outDir = Path.Combine(Path.GetTempPath(), "pervaxis-intake-service-" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(outDir);
        try
        {
            using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
            archive.ExtractToDirectory(outDir);

            var files = Directory.GetFiles(outDir, "*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(outDir, f).Replace('\\', '/'))
                .OrderBy(f => f)
                .ToList();

            // Print all extracted files so they show in test output
            foreach (var file in files)
                Console.WriteLine($"  {file}");

            files.Should().Contain("src/Pervaxis.Clarivolt.Intake/Program.cs");
            files.Should().Contain("src/Pervaxis.Clarivolt.Intake/Controllers/Controller.cs");
            files.Should().Contain("src/Pervaxis.Clarivolt.Intake/Pervaxis.Clarivolt.Intake.csproj");
            files.Should().Contain("tests/Pervaxis.Clarivolt.Intake.Tests/TestBase.cs");
            files.Should().Contain("tests/Pervaxis.Clarivolt.Intake.Tests/Pervaxis.Clarivolt.Intake.Tests.csproj");
            files.Should().Contain("Dockerfile");
            files.Should().Contain(".claude/CLAUDE.md");
            files.Should().Contain(".github/workflows/build-test.yml");
        }
        finally
        {
            Directory.Delete(outDir, recursive: true);
        }
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
