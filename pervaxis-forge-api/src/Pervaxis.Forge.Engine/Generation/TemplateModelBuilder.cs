using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Naming;
using Pervaxis.Forge.Engine.Templating;

namespace Pervaxis.Forge.Engine.Generation;

public static class TemplateModelBuilder
{
    public static TemplateModel Build(ForgeManifest manifest)
    {
        ArgumentNullException.ThrowIfNull(manifest);

        return new TemplateModel
        {
            Manifest = manifest,
            DerivedNames = NamingConvention.DeriveDotNetNames(manifest.Product, manifest.ServiceName),
        };
    }
}
