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

    /// <summary>Returns the full embedded-resource name for every template under <paramref name="templateRoot"/>.</summary>
    /// <remarks>
    /// Returning the full resource name (not just the suffix) ensures that <see cref="LoadAsync"/> never
    /// accidentally matches a same-named template in a different root (e.g. cdk/Program vs rest-api/Program).
    /// </remarks>
    public IReadOnlyCollection<string> GetTemplateSuffixes(string templateRoot)
    {
        if (string.IsNullOrWhiteSpace(templateRoot))
            throw new ArgumentException("Template root must be provided.", nameof(templateRoot));

        var root = templateRoot.Trim().Replace('\\', '.').Replace('/', '.');
        var prefixes = new[]
        {
            $"{root}.",
            $"{root.Replace('-', '_')}.",
        };
        var resourceNames = assembly.GetManifestResourceNames();

        return resourceNames
            .Where(name => name.EndsWith(".sbn", StringComparison.OrdinalIgnoreCase)
                && prefixes.Any(prefix => name.IndexOf(prefix, StringComparison.OrdinalIgnoreCase) >= 0))
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>Returns the path-like suffix of a full resource name relative to the given root prefix.</summary>
    internal static string GetRelativeSuffix(string fullResourceName, string templateRoot)
    {
        var root = templateRoot.Trim().Replace('\\', '.').Replace('/', '.');
        var prefixes = new[]
        {
            $"{root}.",
            $"{root.Replace('-', '_')}.",
        };
        foreach (var prefix in prefixes)
        {
            var idx = fullResourceName.IndexOf(prefix, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
                return fullResourceName[(idx + prefix.Length)..];
        }
        return fullResourceName;
    }
}
