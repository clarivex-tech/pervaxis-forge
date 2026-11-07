namespace Pervaxis.Forge.Engine.Manifest;

public sealed record ResilienceConfig
{
    public bool RetryEnabled { get; init; } = true;
    public bool CircuitBreakerEnabled { get; init; } = true;
    public bool TimeoutEnabled { get; init; } = true;
    public bool InternalHttpClient { get; init; } = true;
    public bool ExternalHttpClient { get; init; } = false;
}
