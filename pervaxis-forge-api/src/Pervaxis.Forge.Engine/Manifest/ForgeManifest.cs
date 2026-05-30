namespace Pervaxis.Forge.Engine.Manifest;

public sealed record ForgeManifest
{
    public required string Product { get; init; }

    public required string VerticalSlug { get; init; }

    public required string ServiceName { get; init; }

    public required ServiceType ServiceType { get; init; }

    public required string ComponentPrefix { get; init; }

    public required string CloudProvider { get; init; }

    public IReadOnlyList<string> GenesisModules { get; init; } = [];

    public IReadOnlyList<string> CanvasModules { get; init; } = [];

    public IReadOnlyList<string> UiTargets { get; init; } = [];

    public DatabaseConfig? Database { get; init; }

    public QueueConfig? Queue { get; init; }

    public ApiConfig? Api { get; init; }

    public AngularConfig? Angular { get; init; }

    public ManifestMetadata? Metadata { get; init; }

    public AuthConfig? Auth { get; init; }

    public ResilienceConfig? Resilience { get; init; }

    public ObservabilityConfig? Observability { get; init; }

    public ValidationConfig? Validation { get; init; }

    public BackgroundJobConfig? BackgroundJobs { get; init; }

    public UtilitiesConfig? Utilities { get; init; }

    public bool MultiTenancy { get; init; } = false;
}
