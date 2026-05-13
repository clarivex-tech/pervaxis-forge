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
            var outputPath = relativeSuffix[..^4].Replace('\\', '/').Replace(".cstemplate", ".cs");
            files.Add(new GeneratedFile(outputPath, rendered));
        }

        return files;
    }
}

public sealed record GeneratedFile(string Path, string Content);
