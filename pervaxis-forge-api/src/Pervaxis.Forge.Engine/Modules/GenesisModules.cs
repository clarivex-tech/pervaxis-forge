namespace Pervaxis.Forge.Engine.Modules;

/// <summary>A Genesis module that a REST print can reference.</summary>
/// <param name="UsesGenesisSection">
/// How the module's <c>AddGenesis*(IConfiguration)</c> method reads its settings. <see langword="true"/>: it reads
/// <c>Genesis:&lt;DisplayName&gt;</c> from the configuration it is given, so the print passes the root configuration.
/// <see langword="false"/>: it binds the section it is given, so the print passes <c>GetSection("&lt;DisplayName&gt;")</c>.
/// Must match the Genesis package; a mismatch silently ignores every setting.
/// </param>
public sealed record GenesisModule(string Id, string DisplayName, string Description, string IamPermissions, bool IsCloudAgnostic = false, bool UsesGenesisSection = false);

public static class GenesisModules
{
    private static readonly GenesisModule[] Modules =
    [
        new("caching", "Caching", "Distributed caching via Redis", "elasticache:*"),
        new("messaging", "Messaging", "Event-driven messaging via SQS/SNS", "sqs:*,sns:*"),
        new("filestorage", "FileStorage", "Object storage via S3", "s3:*"),
        new("search", "Search", "Full-text search via OpenSearch", "opensearch:*"),
        new("notifications", "Notifications", "Email, SMS and push notifications", "sns:*"),
        new("workflow", "Workflow", "Step-based workflow via AWS Step Functions", "stepfunctions:*"),
        new("aiassistance", "AIAssistance", "Generative AI via Bedrock", "bedrock:*"),
        new("reporting", "Reporting", "Data exports and scheduled reports", "athena:*,quicksight:*"),
        new("idempotency", "Idempotency", "Request deduplication via DynamoDB", "dynamodb:*", UsesGenesisSection: true),
        new("odata", "OData", "OData query capabilities for RESTful APIs", "", IsCloudAgnostic: true, UsesGenesisSection: true),
        new("transactionallogging", "TransactionalLogging", "Structured transactional audit logging via CloudWatch", "cloudwatch:*", UsesGenesisSection: true),
        new("featureflags", "FeatureFlags", "Feature flag management via AWS AppConfig", "appconfig:*", UsesGenesisSection: true),
        new("sanitization", "Sanitization", "Server-side input sanitization and XSS prevention", "", IsCloudAgnostic: true, UsesGenesisSection: true),
    ];

    public static IReadOnlyList<GenesisModule> GetAll() => Modules;

    public static GenesisModule? GetById(string id)
        => Modules.FirstOrDefault(module =>
            string.Equals(module.Id, id, StringComparison.OrdinalIgnoreCase)
            || string.Equals(module.DisplayName, id, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> GetAllNames() => Modules.Select(module => module.DisplayName).ToArray();

    public static string GetPackageName(string moduleName, string cloudProvider)
    {
        var module = GetById(moduleName);
        var segment = module?.DisplayName ?? NormalizeModuleName(moduleName);

        if (module?.IsCloudAgnostic == true)
            return $"Pervaxis.Genesis.{segment}";

        return $"Pervaxis.Genesis.{segment}.{NormalizeCloudProvider(cloudProvider)}";
    }

    /// <summary>True when the module reads its settings from <c>Genesis:&lt;DisplayName&gt;</c> (see <see cref="GenesisModule"/>).</summary>
    public static bool UsesGenesisSection(string moduleName) => GetById(moduleName)?.UsesGenesisSection == true;

    /// <summary>The configuration key the module's settings live under, for example <c>Genesis:OData</c> or <c>Caching</c>.</summary>
    public static string GetConfigurationKey(string moduleName)
    {
        var module = GetById(moduleName);
        var name = module?.DisplayName ?? NormalizeModuleName(moduleName);
        return module?.UsesGenesisSection == true ? $"Genesis:{name}" : name;
    }

    public static string GetDiExtensionName(string moduleName, string cloudProvider)
    {
        var module = GetById(moduleName);
        var segment = module?.DisplayName ?? NormalizeModuleName(moduleName);
        return $"AddGenesis{segment}";
    }

    private static string NormalizeSegment(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }

    private static string NormalizeModuleName(string value)
    {
        var segment = NormalizeSegment(value);
        if (!segment.Any(char.IsWhiteSpace) && !segment.Contains('-') && !segment.Contains('_'))
            return segment;

        return string.Concat(
            segment.Split(new[] { '-', ' ', '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpperInvariant(part[0]) + part[1..].ToLowerInvariant()));
    }

    private static string NormalizeCloudProvider(string value)
    {
        return NormalizeSegment(value);
    }
}
