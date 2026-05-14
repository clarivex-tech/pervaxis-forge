namespace Pervaxis.Forge.Engine.Manifest;

public sealed record AngularConfig
{
    public string? WorkspaceName { get; init; }

    public string? ProjectName { get; init; }

    public string? RoutePrefix { get; init; }
}
