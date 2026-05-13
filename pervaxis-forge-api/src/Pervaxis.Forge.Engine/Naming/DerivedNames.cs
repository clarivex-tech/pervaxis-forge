namespace Pervaxis.Forge.Engine.Naming;

public sealed record DerivedNames
{
    public required string DotNetNamespace { get; init; }

    public required string DotNetClassName { get; init; }

    public required string AngularShellComponentName { get; init; }

    public required string AngularMfeComponentName { get; init; }

    public required string AngularShellRoutePath { get; init; }

    public required string AngularMfeRoutePath { get; init; }

    public required string ProjectFile { get; init; }

    public required string TestProjectName { get; init; }

    public required string SolutionFile { get; init; }

    public required string ApiBaseRoute { get; init; }

    public required string DatabaseSchema { get; init; }

    public required string SqsPrefix { get; init; }

    public required string CachePrefix { get; init; }

    public required string DockerImage { get; init; }

    public required string EcsTaskName { get; init; }

    public required string FolderName { get; init; }

    public required string GitHubRepoPath { get; init; }
}
