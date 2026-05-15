using System.Text.RegularExpressions;
using Pervaxis.Forge.Engine.Manifest;

namespace Pervaxis.Forge.Engine.Validation;

public sealed class ManifestValidator
{
    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);

    public ValidationResult Validate(ForgeManifest manifest)
    {
        var errors = new List<string>();

        if (manifest is null)
        {
            return new ValidationResult { Errors = ["Manifest is required."] };
        }

        if (!IsKebabCase(manifest.VerticalSlug))
            errors.Add("VerticalSlug must be kebab-case.");

        if (!IsKebabCase(manifest.ServiceName))
            errors.Add("ServiceName must be kebab-case.");

        if (manifest.ServiceType == ServiceType.RestApi && !manifest.ServiceName.EndsWith("-service", StringComparison.OrdinalIgnoreCase))
            errors.Add(".NET services must end with -service.");

        if (manifest.ServiceType == ServiceType.GraphQL && !manifest.ServiceName.EndsWith("-service", StringComparison.OrdinalIgnoreCase))
            errors.Add("GraphQL services must end with -service.");

        if (manifest.ServiceType == ServiceType.AngularShell && !manifest.ServiceName.EndsWith("-shell", StringComparison.OrdinalIgnoreCase))
            errors.Add("Angular Shell apps must end with -shell (e.g. claims-shell).");

        if (manifest.ServiceType == ServiceType.AngularMfe && manifest.ServiceName.EndsWith("-shell", StringComparison.OrdinalIgnoreCase))
            errors.Add("Angular MFE names must not end with -shell.");

        if (manifest.ServiceType == ServiceType.AngularMfe && manifest.ServiceName.EndsWith("-service", StringComparison.OrdinalIgnoreCase))
            errors.Add("Angular MFE names must not end with -service.");

        if (string.IsNullOrWhiteSpace(manifest.Product))
            errors.Add("Product is required.");

        if (string.IsNullOrWhiteSpace(manifest.ComponentPrefix))
            errors.Add("ComponentPrefix is required.");

        if (string.IsNullOrWhiteSpace(manifest.CloudProvider))
            errors.Add("CloudProvider is required.");

        return new ValidationResult { Errors = errors };
    }

    private static bool IsKebabCase(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && KebabCaseRegex.IsMatch(value);
    }
}
