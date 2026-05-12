using System.IO.Compression;

namespace Pervaxis.Forge.Engine.Generation;

public sealed class ZipPackager
{
    public byte[] Package(IEnumerable<GeneratedFile> files)
    {
        ArgumentNullException.ThrowIfNull(files);

        using var output = new MemoryStream();
        using (var archive = new ZipArchive(output, ZipArchiveMode.Create, leaveOpen: true))
        {
            foreach (var file in files)
            {
                var entry = archive.CreateEntry(file.Path, CompressionLevel.Optimal);
                using var stream = entry.Open();
                using var writer = new StreamWriter(stream);
                writer.Write(file.Content);
            }
        }

        return output.ToArray();
    }
}
