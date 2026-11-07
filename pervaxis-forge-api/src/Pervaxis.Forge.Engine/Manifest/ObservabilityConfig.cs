namespace Pervaxis.Forge.Engine.Manifest;

public sealed record ObservabilityConfig
{
    public bool SerilogEnabled { get; init; } = true;
    public bool CloudWatchEnabled { get; init; } = true;
    public bool OpenTelemetryEnabled { get; init; } = true;
    public bool CorrelationIdEnabled { get; init; } = true;
    public bool PrometheusEnabled { get; init; } = false;
}
