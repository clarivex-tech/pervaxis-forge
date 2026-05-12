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

    public static GenesisModule? GetById(string id) => Modules.FirstOrDefault(module => module.Id == id);

    public static IReadOnlyList<string> GetAllNames() => Modules.Select(module => module.DisplayName).ToArray();

    public static string GetPackageName(string moduleName, string cloudProvider) => $"Pervaxis.Genesis.{moduleName}.{cloudProvider}";

    public static string GetDiExtensionName(string moduleName, string cloudProvider) => $"AddGenesis{moduleName}{cloudProvider}";
}
