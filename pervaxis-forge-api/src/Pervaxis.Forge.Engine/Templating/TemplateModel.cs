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

    public string CurrentYear { get; init; } = DateTime.UtcNow.Year.ToString();
}
