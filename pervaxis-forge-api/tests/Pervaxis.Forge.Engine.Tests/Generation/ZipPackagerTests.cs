using System.IO.Compression;
using FluentAssertions;
using Pervaxis.Forge.Engine.Generation;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public class ZipPackagerTests
{
    private readonly ZipPackager packager = new();

    [Fact]
    public void Package_CreatesZipWithExpectedEntries()
    {
        var files = new[]
        {
            new GeneratedFile("Program.cs", "class Program {}"),
            new GeneratedFile("README.md", "# Test"),
        };

        var zipBytes = packager.Package(files);

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        archive.Entries.Should().HaveCount(2);
        archive.GetEntry("Program.cs")!.Open().CanRead.Should().BeTrue();
        archive.GetEntry("README.md")!.Open().CanRead.Should().BeTrue();
    }
}
