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

        if (manifest.ServiceType == ServiceType.Grpc && !manifest.ServiceName.EndsWith("-service", StringComparison.OrdinalIgnoreCase))
            errors.Add("gRPC services must end with -service.");

        if (manifest.ServiceType == ServiceType.AngularShell && !manifest.ServiceName.EndsWith("-shell", StringComparison.OrdinalIgnoreCase))
            errors.Add("Angular Shell apps must end with -shell (e.g. claims-shell).");

        if ((manifest.ServiceType == ServiceType.AngularMfe || manifest.ServiceType == ServiceType.AngularMfeRemote) && manifest.ServiceName.EndsWith("-shell", StringComparison.OrdinalIgnoreCase))
            errors.Add("Angular MFE names must not end with -shell.");

        if ((manifest.ServiceType == ServiceType.AngularMfe || manifest.ServiceType == ServiceType.AngularMfeRemote) && manifest.ServiceName.EndsWith("-service", StringComparison.OrdinalIgnoreCase))
            errors.Add("Angular MFE names must not end with -service.");

        if ((manifest.ServiceType == ServiceType.AngularShell || manifest.ServiceType == ServiceType.AngularMfe || manifest.ServiceType == ServiceType.AngularMfeRemote) && manifest.UiTargets.Count > 0)
            errors.Add("Angular Shell and Angular MFE requests must not include uiTargets.");

        if (manifest.ServiceType == ServiceType.Monolithic)
        {
            if (manifest.UiTargets.Count == 0)
                errors.Add("Monolithic requests must include uiTargets.");
            else if (!manifest.UiTargets.SequenceEqual(["web"]) && !manifest.UiTargets.SequenceEqual(["web", "mobile"]))
                errors.Add("Monolithic requests must use uiTargets [\"web\"] or [\"web\", \"mobile\"].");
        }

        if (manifest.ServiceType == ServiceType.Ionic)
        {
            if (manifest.UiTargets.Count == 0)
                errors.Add("Ionic requests must include uiTargets.");
            else if (!manifest.UiTargets.SequenceEqual(["mobile"]))
                errors.Add("Ionic requests must use uiTargets [\"mobile\"].");
        }

        if (string.IsNullOrWhiteSpace(manifest.Product))
            errors.Add("Product is required.");

        if (string.IsNullOrWhiteSpace(manifest.ComponentPrefix))
            errors.Add("ComponentPrefix is required.");

        if (string.IsNullOrWhiteSpace(manifest.CloudProvider))
            errors.Add("CloudProvider is required.");

        if (manifest.Observability is { CloudWatchEnabled: true, SerilogEnabled: false })
            errors.Add("Observability.CloudWatchEnabled requires Observability.SerilogEnabled — CloudWatch is a Serilog sink.");

        if (manifest.MultiTenancy && manifest.Database is null)
            errors.Add("MultiTenancy requires a Database configuration (tenant-scoped DbContext needs a DB).");

        if (manifest.BackgroundJobs?.Provider is { } provider && provider is not ("EventBridge" or "SqsLambda"))
            errors.Add($"BackgroundJobs.Provider '{provider}' is not recognized. Accepted values: EventBridge, SqsLambda.");

        return new ValidationResult { Errors = errors };
    }

    private static bool IsKebabCase(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && KebabCaseRegex.IsMatch(value);
    }
}
