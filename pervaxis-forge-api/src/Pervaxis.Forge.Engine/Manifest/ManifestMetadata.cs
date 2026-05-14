namespace Pervaxis.Forge.Engine.Manifest;

public sealed record ManifestMetadata
{
    public string? Author { get; init; }

    public string? Description { get; init; }

    public string? Version { get; init; }

    public string? CreatedBy { get; init; }

    public DateTimeOffset? CreatedAtUtc { get; init; }
}
