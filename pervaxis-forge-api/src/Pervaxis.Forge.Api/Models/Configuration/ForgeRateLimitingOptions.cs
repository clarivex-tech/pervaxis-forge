namespace Pervaxis.Forge.Api.Models.Configuration;

public sealed class ForgeRateLimitingOptions
{
    public const string SectionName = "Forge:RateLimiting";

    public bool Enabled { get; init; } = false;

    public int PermitLimit { get; init; } = 100;

    public int WindowMinutes { get; init; } = 1;
}
