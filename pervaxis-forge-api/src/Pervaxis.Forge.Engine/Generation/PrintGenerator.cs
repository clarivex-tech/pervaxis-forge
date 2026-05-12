using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Templating;

namespace Pervaxis.Forge.Engine.Generation;

public sealed class PrintGenerator
{
    private readonly FileGenerator fileGenerator;
    private readonly ZipPackager zipPackager;

    public PrintGenerator(FileGenerator? fileGenerator = null, ZipPackager? zipPackager = null)
    {
        this.fileGenerator = fileGenerator ?? new FileGenerator();
        this.zipPackager = zipPackager ?? new ZipPackager();
    }

    public async Task<byte[]> GenerateAsync(ForgeManifest manifest, string templateRoot, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        var model = TemplateModelBuilder.Build(manifest);
        var files = await fileGenerator.GenerateAsync(templateRoot, model, cancellationToken);
        return zipPackager.Package(files);
    }
}
