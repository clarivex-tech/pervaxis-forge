using System;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Modules;
using Pervaxis.Forge.Engine.Naming;
using Pervaxis.Forge.Engine.Templating;

namespace Pervaxis.Forge.Engine.Generation;

public static class TemplateModelBuilder
{
    public static TemplateModel Build(ForgeManifest manifest, string cloudProvider)
    {
        ArgumentNullException.ThrowIfNull(manifest);
        ArgumentException.ThrowIfNullOrWhiteSpace(cloudProvider);

        var selectedModules = manifest.GenesisModules
            .Select(moduleName => new SelectedModule
            {
                Name = moduleName,
                PackageName = GenesisModules.GetPackageName(moduleName, cloudProvider),
                DiExtensionName = GenesisModules.GetDiExtensionName(moduleName, cloudProvider),
            })
            .ToList();

        var selectedCanvasModules = manifest.CanvasModules
            .Select(moduleName => new SelectedCanvasModule
            {
                Name = moduleName,
                PackageName = CanvasModules.GetPackageName(moduleName),
                ImportName = CanvasModules.GetImportName(moduleName),
            })
            .ToList();

        return new TemplateModel
        {
            Manifest = manifest,
            DerivedNames = BuildDerivedNames(manifest),
            CloudProvider = cloudProvider,
            SelectedModules = selectedModules,
            SelectedCanvasModules = selectedCanvasModules,
            CurrentYear = DateTime.UtcNow.Year.ToString(),
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
            ProjectFile = dotNetNames.ProjectFile,
            TestProjectName = dotNetNames.TestProjectName,
            SolutionFile = dotNetNames.SolutionFile,
            ApiBaseRoute = dotNetNames.ApiBaseRoute,
            DatabaseSchema = dotNetNames.DatabaseSchema,
            SqsPrefix = dotNetNames.SqsPrefix,
            CachePrefix = dotNetNames.CachePrefix,
            DockerImage = dotNetNames.DockerImage,
            EcsTaskName = dotNetNames.EcsTaskName,
            FolderName = dotNetNames.FolderName,
            GitHubRepoPath = dotNetNames.GitHubRepoPath,
        };
    }
}
