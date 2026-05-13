using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Naming;
using Pervaxis.Forge.Engine.Templating;

namespace Pervaxis.Forge.Engine.Generation;

public static class TemplateModelBuilder
{
    public static TemplateModel Build(ForgeManifest manifest, string cloudProvider)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(cloudProvider);

        return new TemplateModel
        {
            Manifest = manifest,
            DerivedNames = BuildDerivedNames(manifest),
            CloudProvider = cloudProvider,
        };
    }

    private static DerivedNames BuildDerivedNames(ForgeManifest manifest)
    {
        var dotNetNames = NamingConvention.DeriveDotNetNames(manifest.Product, manifest.ServiceName);
        var shellNames = NamingConvention.DeriveAngularShellNames(manifest.Product, manifest.ServiceName);
        var mfeNames = NamingConvention.DeriveAngularMfeNames(manifest.Product, manifest.ServiceName);

        return new DerivedNames
        {
            DotNetNamespace = dotNetNames.DotNetNamespace,
            DotNetClassName = dotNetNames.DotNetClassName,
            AngularShellComponentName = shellNames.AngularShellComponentName,
            AngularMfeComponentName = mfeNames.AngularMfeComponentName,
            AngularShellRoutePath = shellNames.AngularShellRoutePath,
            AngularMfeRoutePath = mfeNames.AngularMfeRoutePath,
        };
    }
}
