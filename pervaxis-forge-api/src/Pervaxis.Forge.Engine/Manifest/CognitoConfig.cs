using System.Text.Json.Serialization;

namespace Pervaxis.Forge.Engine.Manifest;

/// <summary>
/// Amazon Cognito settings that are written into the generated service's appsettings.json.
/// Only identifiers are stored here (no secrets). The service validates tokens and needs no client secret.
/// </summary>
public sealed record CognitoConfig
{
    /// <summary>The user pool ID, for example <c>us-east-1_AbC123xyz</c>. The AWS region is the part before the underscore.</summary>
    public required string UserPoolId { get; init; }

    /// <summary>The app client IDs allowed to call the service (for example the web app and the mobile app).</summary>
    public IReadOnlyList<string> ClientIds { get; init; } = [];

    /// <summary>The Cognito group allowed to use the built-in AdminOnly policy.</summary>
    public string AdminGroup { get; init; } = "Admin";

    /// <summary>When true the service accepts API keys as well as bearer tokens (scheme Both).</summary>
    public bool AllowApiKey { get; init; }

    [JsonIgnore]
    public string Region => UserPoolId.Contains('_') ? UserPoolId[..UserPoolId.IndexOf('_')] : string.Empty;

    [JsonIgnore]
    public string Authority => $"https://cognito-idp.{Region}.amazonaws.com/{UserPoolId}";
}
