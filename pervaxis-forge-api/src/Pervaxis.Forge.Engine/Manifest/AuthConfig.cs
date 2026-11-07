namespace Pervaxis.Forge.Engine.Manifest;

public sealed record AuthConfig
{
    public bool ApiKeyEnabled { get; init; } = true;
    public bool JwtEnabled { get; init; } = false;
    public bool MtlsEnabled { get; init; } = false;
}
