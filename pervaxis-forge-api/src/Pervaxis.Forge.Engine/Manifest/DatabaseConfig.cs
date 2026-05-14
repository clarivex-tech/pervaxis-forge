namespace Pervaxis.Forge.Engine.Manifest;

public sealed record DatabaseConfig
{
    public string? Engine { get; init; }

    public string? Name { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string? Host { get; init; }

    public int? Port { get; init; }

    public string? SslMode { get; init; }
}
