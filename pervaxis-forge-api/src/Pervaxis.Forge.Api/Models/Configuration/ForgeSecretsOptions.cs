namespace Pervaxis.Forge.Api.Models.Configuration;

/// <summary>
/// Secret source configuration for the API key authentication baseline.
/// </summary>
public sealed class ForgeSecretsOptions
{
    public const string SectionName = "Forge:Secrets";

    /// <summary>
    /// Enables Secrets Manager lookup instead of using a local inline API key.
    /// </summary>
    public bool UseSecretsManager { get; init; }

    /// <summary>
    /// AWS Secrets Manager secret identifier or ARN.
    /// </summary>
    public string? SecretId { get; init; }

    /// <summary>
    /// JSON property name inside the secret that contains the API key value.
    /// </summary>
    public string ApiKeySecretKey { get; init; } = "Forge:Authentication:ApiKey";
}
