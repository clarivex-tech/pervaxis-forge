namespace Pervaxis.Forge.Engine.Manifest;

public sealed record ValidationConfig
{
    public bool FluentValidationEnabled { get; init; } = true;
}
