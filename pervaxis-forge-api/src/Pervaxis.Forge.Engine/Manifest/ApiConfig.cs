namespace Pervaxis.Forge.Engine.Manifest;

public sealed record ApiConfig
{
    public string? BasePath { get; init; }

    public bool SwaggerEnabled { get; init; }

    public bool CorsEnabled { get; init; }
}
