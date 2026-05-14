namespace Pervaxis.Forge.Engine.Manifest;

public sealed record QueueConfig
{
    public string? Provider { get; init; }

    public string? Name { get; init; }

    public bool Enabled { get; init; }

    public int? MaxRetries { get; init; }
}
