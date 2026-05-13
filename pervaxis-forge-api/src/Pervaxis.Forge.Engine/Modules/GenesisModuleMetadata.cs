namespace Pervaxis.Forge.Engine.Modules;

public sealed record GenesisModuleMetadata
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    public required string IamPermissions { get; init; }
}
