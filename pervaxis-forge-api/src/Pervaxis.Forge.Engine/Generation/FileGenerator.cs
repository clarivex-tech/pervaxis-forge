using Pervaxis.Forge.Engine.Templating;

namespace Pervaxis.Forge.Engine.Generation;

public sealed class FileGenerator
{
    private readonly TemplateLoader templateLoader;
    private readonly ITemplateEngine templateEngine;

    public FileGenerator(TemplateLoader? templateLoader = null, ITemplateEngine? templateEngine = null)
    {
        this.templateLoader = templateLoader ?? new TemplateLoader();
        this.templateEngine = templateEngine ?? new ScribanTemplateEngine();
    }

    public async Task<IReadOnlyCollection<GeneratedFile>> GenerateAsync(
        string templateRoot,
        TemplateModel model,
        CancellationToken cancellationToken = default)
    {
        var resourceNames = templateLoader.GetTemplateSuffixes(templateRoot);
        var files = new List<GeneratedFile>(resourceNames.Count);

        foreach (var resourceName in resourceNames)
        {
            var template = await templateLoader.LoadAsync(resourceName, cancellationToken);
            var rendered = templateEngine.Render(template, model);
            var relativeSuffix = TemplateLoader.GetRelativeSuffix(resourceName, templateRoot);
            // Strip .sbn, normalise path separators, then map .cstemplate → .cs
            var outputPath = relativeSuffix[..^4].Replace('\\', '/').Replace(".cstemplate", ".cs");
            // Substitute folder name tokens with derived project names
            var projectName = model.DerivedNames.ProjectFile[..^7]; // strip .csproj
            outputPath = outputPath
                .Replace("__PROJECT__", projectName)
                .Replace("__TEST_PROJECT__", model.DerivedNames.TestProjectName);
            // csproj.sbn and tests.csproj.sbn use generic names; replace with derived project names
            outputPath = outputPath switch
            {
                var p when p.EndsWith("/csproj") => p[..^6] + model.DerivedNames.ProjectFile,
                var p when p.EndsWith("/tests.csproj") => p[..^12] + model.DerivedNames.TestProjectName + ".csproj",
                "Protos/service.proto" => $"Protos/{model.Manifest.ServiceName}.proto",
                _ => outputPath,
            };
            files.Add(new GeneratedFile(outputPath, rendered));
        }

        return files;
    }
}

public sealed record GeneratedFile(string Path, string Content);
