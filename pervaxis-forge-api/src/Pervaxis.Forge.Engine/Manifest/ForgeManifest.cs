namespace Pervaxis.Forge.Engine.Manifest;

public sealed record ForgeManifest
{
    public required string Product { get; init; }

    public required string VerticalSlug { get; init; }

    public required string ServiceName { get; init; }

    public required ServiceType ServiceType { get; init; }

    public required string ComponentPrefix { get; init; }

    public required string CloudProvider { get; init; }

    public DatabaseConfig? Database { get; init; }

    public QueueConfig? Queue { get; init; }

    public ApiConfig? Api { get; init; }

    public AngularConfig? Angular { get; init; }

    public ManifestMetadata? Metadata { get; init; }
}
