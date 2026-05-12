using System.Reflection;

namespace Pervaxis.Forge.Engine.Templating;

public sealed class TemplateLoader
{
    private readonly Assembly assembly;

    public TemplateLoader(Assembly? assembly = null)
    {
        this.assembly = assembly ?? typeof(TemplateLoader).Assembly;
    }

    public async Task<string> LoadAsync(string resourceSuffix, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceSuffix))
            throw new ArgumentException("Resource suffix must be provided.", nameof(resourceSuffix));

        var resourceName = assembly
            .GetManifestResourceNames()
            .FirstOrDefault(name => name.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
            throw new FileNotFoundException($"Template resource '{resourceSuffix}' was not found.");

        await using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Template resource '{resourceSuffix}' could not be opened.");

        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    public IReadOnlyCollection<string> GetTemplateSuffixes(string templateRoot)
    {
        if (string.IsNullOrWhiteSpace(templateRoot))
            throw new ArgumentException("Template root must be provided.", nameof(templateRoot));

        var prefix = $"{templateRoot.Trim().Replace('\\', '.')}.";

        return assembly
            .GetManifestResourceNames()
            .Where(name => name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .Select(name => name[prefix.Length..])
            .Where(suffix => suffix.EndsWith(".sbn", StringComparison.OrdinalIgnoreCase))
            .OrderBy(suffix => suffix, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
