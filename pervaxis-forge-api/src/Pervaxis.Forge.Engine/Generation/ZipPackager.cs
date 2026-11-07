using System.IO.Compression;
using System.Text;

namespace Pervaxis.Forge.Engine.Generation;

public sealed class ZipPackager
{
    private static readonly Encoding Utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

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
                using var writer = new StreamWriter(stream, Utf8NoBom);
                writer.Write(file.Content);
            }
        }

        return output.ToArray();
    }
}
