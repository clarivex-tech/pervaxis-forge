namespace Pervaxis.Forge.Engine.Modules;

public sealed record GenesisModule(string Id, string DisplayName, string IamPermissions);

public static class GenesisModules
{
    private static readonly GenesisModule[] Modules =
    [
        new("caching", "Caching", "elasticache:*"),
        new("messaging", "Messaging", "sqs:*,sns:*"),
        new("filestorage", "FileStorage", "s3:*"),
        new("search", "Search", "opensearch:*"),
        new("notifications", "Notifications", "sns:*"),
        new("workflow", "Workflow", "stepfunctions:*"),
        new("aiassistance", "AIAssistance", "bedrock:*"),
        new("reporting", "Reporting", "athena:*,quicksight:*"),
    ];

    public static IReadOnlyList<GenesisModule> GetAll() => Modules;

    public static GenesisModule? GetById(string id)
        => Modules.FirstOrDefault(module => string.Equals(module.Id, id, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> GetAllNames() => Modules.Select(module => module.DisplayName).ToArray();

    public static string GetPackageName(string moduleName, string cloudProvider)
    {
        var module = GetById(moduleName);
        var segment = module?.DisplayName ?? NormalizeModuleName(moduleName);
        return $"Pervaxis.Genesis.{segment}.{NormalizeCloudProvider(cloudProvider)}";
    }

    public static string GetDiExtensionName(string moduleName, string cloudProvider)
    {
        var module = GetById(moduleName);
        var segment = module?.DisplayName ?? NormalizeModuleName(moduleName);
        return $"AddGenesis{segment}{NormalizeCloudProvider(cloudProvider)}";
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
