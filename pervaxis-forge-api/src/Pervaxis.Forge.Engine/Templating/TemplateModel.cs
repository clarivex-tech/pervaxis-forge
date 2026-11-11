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
