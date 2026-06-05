using System.Net.Http.Json;
using System.Text.Json;

namespace Pervaxis.Forge.Engine.NuGet;

/// <summary>
/// Resolves the latest stable version of a NuGet package by querying
/// the nuget.org Registration API (no auth required).
/// Results are cached in-memory for 15 minutes to avoid hammering the feed.
/// </summary>
public sealed class NuGetVersionResolver : INuGetVersionResolver
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);
    private static readonly string FallbackVersion = "3.2.0";

    private readonly HttpClient httpClient;
    private readonly Dictionary<string, (string Version, DateTime ExpiresAt)> cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly SemaphoreSlim semaphore = new(1, 1);

    public NuGetVersionResolver(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task<string> GetLatestVersionAsync(string packageId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(packageId);

        // Check cache first
        if (TryGetCached(packageId, out var cached))
            return cached;

        await semaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring lock
            if (TryGetCached(packageId, out cached))
                return cached;

            var version = await FetchLatestVersionAsync(packageId, cancellationToken);
            cache[packageId] = (version, DateTime.UtcNow.Add(CacheDuration));
            return version;
        }
        finally
        {
            semaphore.Release();
        }
    }

    private bool TryGetCached(string packageId, out string version)
    {
        if (cache.TryGetValue(packageId, out var entry) && DateTime.UtcNow < entry.ExpiresAt)
        {
            version = entry.Version;
            return true;
        }

        version = string.Empty;
        return false;
    }

    private async Task<string> FetchLatestVersionAsync(string packageId, CancellationToken cancellationToken)
    {
        try
        {
            // nuget.org v3 flat container API: returns all versions for a package
            var url = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/index.json";
            var response = await httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return FallbackVersion;

            var json = await response.Content.ReadFromJsonAsync<VersionIndex>(cancellationToken: cancellationToken);
            if (json?.Versions is null || json.Versions.Length == 0)
                return FallbackVersion;

            // Filter out pre-release versions (those containing '-') and take the last stable version
            var latestStable = json.Versions
                .Where(v => !v.Contains('-'))
                .LastOrDefault();

            return latestStable ?? json.Versions[^1];
        }
        catch (Exception)
        {
            // Network failures, timeouts, deserialization errors — fall back gracefully
            return FallbackVersion;
        }
    }

    private sealed record VersionIndex(string[] Versions);
}
