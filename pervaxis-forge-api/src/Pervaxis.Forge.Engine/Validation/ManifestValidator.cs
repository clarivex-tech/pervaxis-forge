using System.Text.RegularExpressions;
using Pervaxis.Forge.Engine.Manifest;

namespace Pervaxis.Forge.Engine.Validation;

public sealed class ManifestValidator
{
    // Every pattern has a match timeout so unusual input cannot make validation run for a long time.
    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(250);

    private static readonly Regex KebabCaseRegex = new("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled, RegexTimeout);

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

        if (manifest.ServiceType == ServiceType.RestApi && !manifest.MultiTenancy)
            errors.Add("REST API prints require MultiTenancy. Single-tenant mode is not supported.");
        if (manifest.ServiceType == ServiceType.RestApi && (manifest.InitialTenantId is null || manifest.InitialTenantId == Guid.Empty))
            errors.Add("REST API prints require a non-empty InitialTenantId GUID shared by related services.");
        if (manifest.MultiTenancy)
            ValidateMultiTenancy(manifest, errors);
        if (manifest.ServiceType == ServiceType.Monolithic && manifest.AngularAuth is null)
            errors.Add("Angular monolith prints require AngularAuth with Cognito settings.");

        if (manifest.BackgroundJobs?.Provider is { } provider && provider is not ("EventBridge" or "SqsLambda"))
            errors.Add($"BackgroundJobs.Provider '{provider}' is not recognized. Accepted values: EventBridge, SqsLambda.");

        if (manifest.Cognito is { } cognito)
            ValidateCognito(manifest, cognito, errors);

        if (manifest.AngularAuth is { } angularAuth)
            ValidateAngularAuth(manifest, angularAuth, errors);

        if (manifest.ServiceType == ServiceType.IdentityTokenEnrichment)
        {
            if (manifest.CloudProvider != "AWS")
                errors.Add("Identity Token Enrichment requires the AWS cloud provider.");
            if (manifest.TokenEnrichment is not { } enrichment)
                errors.Add("TokenEnrichment settings are required.");
            else
            {
                if (string.IsNullOrWhiteSpace(enrichment.Region) || !Regex.IsMatch(enrichment.Region, @"^[a-z]{2}(?:-[a-z]+)+-\d+$", RegexOptions.None, RegexTimeout))
                    errors.Add("TokenEnrichment.Region must be an AWS region.");
                if (string.IsNullOrWhiteSpace(enrichment.UserPoolId) || !UserPoolIdRegex.IsMatch(enrichment.UserPoolId))
                    errors.Add("TokenEnrichment.UserPoolId must be a Cognito user pool ID.");
                else if (!enrichment.UserPoolId.StartsWith(enrichment.Region + "_", StringComparison.Ordinal))
                    errors.Add("TokenEnrichment user pool must belong to the selected region.");
            }
            if (manifest.Database is not null || manifest.GenesisModules.Count > 0 || manifest.CanvasModules.Count > 0 || manifest.UiTargets.Count > 0)
                errors.Add("Identity Token Enrichment does not support database, Genesis, Canvas or UI settings.");
        }
        else if (manifest.TokenEnrichment is not null)
            errors.Add("TokenEnrichment settings are only supported for Identity Token Enrichment prints.");

        return new ValidationResult { Errors = errors };
    }

    // These values are written into appsettings.json, so the patterns also keep them safe for JSON.
    private static readonly Regex UserPoolIdRegex = new(@"^[a-z]{2}(?:-[a-z]+)+-\d+_[A-Za-z0-9]+$", RegexOptions.Compiled, RegexTimeout);
    private static readonly Regex ClientIdRegex = new(@"^[A-Za-z0-9_+]{1,128}$", RegexOptions.Compiled, RegexTimeout);
    private static readonly Regex GroupNameRegex = new(@"^[A-Za-z0-9_.:/@+= -]{1,128}$", RegexOptions.Compiled, RegexTimeout);

    /// <summary>
    /// Multi-tenancy v1 takes the tenant from a validated Cognito access token and isolates data in PostgreSQL.
    /// API-key-only tenancy and client-credentials tenancy are not supported.
    /// </summary>
    private static void ValidateMultiTenancy(ForgeManifest manifest, List<string> errors)
    {
        if (manifest.ServiceType != ServiceType.RestApi)
            errors.Add("MultiTenancy is only supported for REST API services.");

        if (manifest.Database is null)
            errors.Add("MultiTenancy requires a Database configuration (tenant-scoped DbContext needs a DB).");
        else if (!IsPostgreSql(manifest.Database.Engine))
            errors.Add("MultiTenancy requires a PostgreSQL database (Database.Engine 'postgresql').");

        if (manifest.Cognito is null)
            errors.Add("MultiTenancy requires Cognito JWT authentication (the tenant comes from a validated access token claim).");
        else if (manifest.Cognito.AllowApiKey)
            errors.Add("MultiTenancy does not support API keys (Cognito.AllowApiKey). Tenant identity comes only from the access token.");
    }

    private static bool IsPostgreSql(string? engine) =>
        string.Equals(engine?.Trim(), "postgresql", StringComparison.OrdinalIgnoreCase)
        || string.Equals(engine?.Trim(), "postgres", StringComparison.OrdinalIgnoreCase);

    private static void ValidateCognito(ForgeManifest manifest, CognitoConfig cognito, List<string> errors)
    {
        if (manifest.ServiceType != ServiceType.RestApi)
            errors.Add("Cognito settings are only supported for REST API services.");

        if (!string.Equals(manifest.CloudProvider, "AWS", StringComparison.OrdinalIgnoreCase))
            errors.Add("Cognito requires the AWS cloud provider.");

        if (string.IsNullOrWhiteSpace(cognito.UserPoolId) || !UserPoolIdRegex.IsMatch(cognito.UserPoolId))
            errors.Add("Cognito.UserPoolId must look like 'us-east-1_AbC123xyz'.");

        if (cognito.ClientIds is null || cognito.ClientIds.Count == 0)
            errors.Add("Cognito.ClientIds needs at least one app client ID.");
        else if (cognito.ClientIds.Any(id => string.IsNullOrWhiteSpace(id) || !ClientIdRegex.IsMatch(id)))
            errors.Add("Cognito.ClientIds contains an app client ID with unsupported characters. Use letters, digits, '_' or '+'.");

        if (string.IsNullOrWhiteSpace(cognito.AdminGroup) || !GroupNameRegex.IsMatch(cognito.AdminGroup))
            errors.Add("Cognito.AdminGroup must be 1-128 characters: letters, digits and . _ : / @ + = - or space.");
    }

    private static void ValidateAngularAuth(ForgeManifest manifest, AngularAuthConfig auth, List<string> errors)
    {
        if (manifest.ServiceType != ServiceType.Monolithic)
            errors.Add("AngularAuth is only supported for Angular monoliths.");
        if (auth.Provider != "Cognito")
            errors.Add("Angular monolith prints require AngularAuth.Provider Cognito.");
        if (auth.Provider != "Cognito")
            return;
        if (!string.Equals(manifest.CloudProvider, "AWS", StringComparison.OrdinalIgnoreCase))
            errors.Add("Angular Cognito requires the AWS cloud provider.");
        if (string.IsNullOrWhiteSpace(auth.UserPoolId) || !UserPoolIdRegex.IsMatch(auth.UserPoolId))
            errors.Add("AngularAuth.UserPoolId must look like 'us-east-1_AbC123xyz'.");
        if (string.IsNullOrWhiteSpace(auth.ClientId) || !ClientIdRegex.IsMatch(auth.ClientId))
            errors.Add("AngularAuth.ClientId must be a public app client ID.");
        if (!IsBrowserUrl(auth.Domain, out var domain) || domain!.Scheme != "https" || domain.AbsolutePath != "/")
            errors.Add("AngularAuth.Domain must be an HTTPS origin without a path.");
        if (auth.Scopes is null || !auth.Scopes.Contains("openid") || auth.Scopes.Any(s =>
            string.IsNullOrWhiteSpace(s) || s.Any(c => c < 0x21 || c > 0x7e || c is '"' or '\\')))
            errors.Add("AngularAuth.Scopes must contain openid and valid OAuth scope names.");
        var validRedirect = IsBrowserUrl(auth.RedirectUri, out var redirect);
        if (!validRedirect || redirect!.AbsolutePath != "/auth/callback")
            errors.Add("AngularAuth.RedirectUri must be an absolute HTTPS (or localhost HTTP) URL ending in /auth/callback.");
        if (!IsBrowserUrl(auth.PostLogoutRedirectUri, out var logout) ||
            (validRedirect && redirect!.GetLeftPart(UriPartial.Authority) != logout!.GetLeftPart(UriPartial.Authority)))
            errors.Add("AngularAuth.PostLogoutRedirectUri must be an HTTPS (or localhost HTTP) URL on the callback origin.");
    }

    private static bool IsBrowserUrl(string? value, out Uri? uri) =>
        Uri.TryCreate(value, UriKind.Absolute, out uri) &&
        (uri.Scheme == "https" || (uri.Scheme == "http" && uri.Host is "localhost" or "127.0.0.1" or "[::1]")) &&
        string.IsNullOrEmpty(uri.UserInfo) && string.IsNullOrEmpty(uri.Query) && string.IsNullOrEmpty(uri.Fragment);

    private static bool IsKebabCase(string? value)
    {
        return !string.IsNullOrWhiteSpace(value) && KebabCaseRegex.IsMatch(value);
    }
}
