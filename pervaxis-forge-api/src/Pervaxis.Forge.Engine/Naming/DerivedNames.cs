namespace Pervaxis.Forge.Engine.Naming;

public sealed record DerivedNames
{
    public required string DotNetNamespace { get; init; }

    public required string DotNetClassName { get; init; }

    public required string AngularShellComponentName { get; init; }

    public required string AngularMfeComponentName { get; init; }

    public required string AngularShellRoutePath { get; init; }

    public required string AngularMfeRoutePath { get; init; }
}
