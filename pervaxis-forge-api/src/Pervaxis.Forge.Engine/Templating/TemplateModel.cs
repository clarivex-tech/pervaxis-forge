using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Naming;

namespace Pervaxis.Forge.Engine.Templating;

public sealed record TemplateModel
{
    public required ForgeManifest Manifest { get; init; }

    public required DerivedNames DerivedNames { get; init; }

    public required string CloudProvider { get; init; }
}
