namespace Pervaxis.Forge.Engine.NuGet;

/// <summary>
/// Resolves the latest stable version of a NuGet package from nuget.org.
/// </summary>
public interface INuGetVersionResolver
{
    /// <summary>
    /// Gets the latest stable version for the given package ID.
    /// </summary>
    Task<string> GetLatestVersionAsync(string packageId, CancellationToken cancellationToken = default);
}
