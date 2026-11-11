namespace Pervaxis.Forge.Engine.Templating;

public sealed record SelectedModule
{
    public required string Name { get; init; }
    public required string PackageName { get; init; }
    public required string DiExtensionName { get; init; }
    public required string Version { get; init; }

    /// <summary>The module's display name, as used in configuration keys.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>True when the module reads <c>Genesis:&lt;DisplayName&gt;</c>; the print then passes the root configuration.</summary>
    public bool UsesGenesisSection { get; init; }
}
