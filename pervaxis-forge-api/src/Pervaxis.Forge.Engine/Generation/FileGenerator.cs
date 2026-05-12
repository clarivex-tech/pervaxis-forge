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
        var suffixes = templateLoader.GetTemplateSuffixes(templateRoot);
        var files = new List<GeneratedFile>(suffixes.Count);

        foreach (var suffix in suffixes)
        {
            var template = await templateLoader.LoadAsync(suffix, cancellationToken);
            var rendered = templateEngine.Render(template, model);
            files.Add(new GeneratedFile(suffix[..^4], rendered));
        }

        return files;
    }
}

public sealed record GeneratedFile(string Path, string Content);
