namespace Pervaxis.Forge.Api.Models.Configuration;

public sealed class ForgeAuthenticationOptions
{
    public const string SectionName = "Forge:Authentication";

    public string? ApiKey { get; init; }
}
