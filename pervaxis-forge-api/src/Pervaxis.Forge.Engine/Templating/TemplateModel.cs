using System;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Naming;
using Pervaxis.Forge.Engine.Templating;

namespace Pervaxis.Forge.Engine.Templating;

public sealed record TemplateModel
{
    public required ForgeManifest Manifest { get; init; }

    public required DerivedNames DerivedNames { get; init; }

    public required string CloudProvider { get; init; }

    public IReadOnlyList<SelectedModule> SelectedModules { get; init; } = [];

    public IReadOnlyList<SelectedCanvasModule> SelectedCanvasModules { get; init; } = [];

    public string CurrentYear { get; init; } = DateTime.UtcNow.Year.ToString();

    public string ForgeVersion { get; init; } = Engine.ForgeVersion.Full;

    public string ForgeBuild { get; init; } = Engine.ForgeVersion.Build;

    public string PrintedAt { get; init; } = DateTime.UtcNow.ToString("o");

    public string GenesisVersion { get; init; } = "";

    /// <summary>Angular monolith prints always include Cognito tenant authentication.</summary>
    public bool AngularCognitoEnabled => true;

    /// <summary>The reference domain is only emitted for PostgreSQL tenant-ready REST services.</summary>
    public bool WorkspaceTasksEnabled => Manifest.ServiceType == ServiceType.RestApi
        && Manifest.MultiTenancy && Manifest.Cognito is not null
        && Manifest.Database?.Engine == "postgresql" && Manifest.IncludeWorkspaceTasks != false;

    /// <summary>JSON-escaped public runtime settings for an explicit Cognito selection.</summary>
    public string AngularAppConfigJson => System.Text.Json.JsonSerializer.Serialize(new
    {
        apiUrl = "/api",
        auth = Manifest.AngularAuth is { Provider: "Cognito" } auth ? new
        {
            cognitoDomain = auth.Domain,
            issuer = $"https://cognito-idp.{auth.UserPoolId!.Split('_')[0]}.amazonaws.com/{auth.UserPoolId}",
            clientId = auth.ClientId,
            scopes = auth.Scopes,
            redirectUri = auth.RedirectUri,
            postLogoutRedirectUri = auth.PostLogoutRedirectUri,
            roleClaim = "cognito:groups"
        } : null,
        tenancy = new { mode = "MultiTenant", claimType = "tenant_id" }
    }, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

    /// <summary>
    /// The first tenant of a generated service: seeded into the tenants table and used as the only tenant in
    /// single-tenant mode. One value per print, so every file of the print agrees.
    /// </summary>
    public string DefaultTenantId { get; init; } = Guid.NewGuid().ToString("D");

    /// <summary>
    /// The creation date recorded for the seeded default tenant: the print date at midnight UTC.
    /// Date only, so the EF seed data and its migration stay simple and stable.
    /// </summary>
    public DateOnly DefaultTenantSeededOn { get; init; } = DateOnly.FromDateTime(DateTime.UtcNow);
}
