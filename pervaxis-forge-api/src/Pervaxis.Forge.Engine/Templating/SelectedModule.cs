namespace Pervaxis.Forge.Engine.Templating;

public sealed record SelectedModule
{
    public required string Name { get; init; }
    public required string PackageName { get; init; }
    public required string DiExtensionName { get; init; }
}
