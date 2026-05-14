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
    /// Resource names are path-based (e.g. <c>Templates\rest-api\.github\workflows\build-test.yml.sbn</c>)
    /// because the csproj uses <c>LogicalName="%(Identity)"</c>. Returning the full resource name ensures
    /// <see cref="LoadAsync"/> never matches a same-named template in a different root
    /// (e.g. <c>cdk\Program</c> vs <c>rest-api\Program</c>).
    /// </remarks>
    public IReadOnlyCollection<string> GetTemplateSuffixes(string templateRoot)
    {
        if (string.IsNullOrWhiteSpace(templateRoot))
            throw new ArgumentException("Template root must be provided.", nameof(templateRoot));

        var root = templateRoot.Trim().Replace('\\', '/');
        var resourceNames = assembly.GetManifestResourceNames();

        return resourceNames
            .Where(name =>
            {
                var normalized = name.Replace('\\', '/');
                return normalized.EndsWith(".sbn", StringComparison.OrdinalIgnoreCase)
                    && normalized.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase);
            })
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>Returns the relative output path for a full resource name by stripping the template root prefix and the trailing <c>.sbn</c>.</summary>
    internal static string GetRelativeSuffix(string fullResourceName, string templateRoot)
    {
        var root = templateRoot.Trim().Replace('\\', '/');
        var normalizedFull = fullResourceName.Replace('\\', '/');
        var prefix = root + "/";
        if (normalizedFull.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return normalizedFull[prefix.Length..];
        return fullResourceName;
    }
}
