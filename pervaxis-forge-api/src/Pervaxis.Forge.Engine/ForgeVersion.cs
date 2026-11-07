using System.Reflection;

namespace Pervaxis.Forge.Engine;

/// <summary>
/// Provides the Forge engine version and build metadata.
/// Version is stamped by CI via AssemblyInformationalVersion.
/// Format: {Major}.{Minor}.{Patch}+{yyyyMMdd}.{shortSha}
/// </summary>
public static class ForgeVersion
{
    private static readonly Lazy<string> VersionLazy = new(() =>
    {
        var attr = typeof(ForgeVersion).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        return attr?.InformationalVersion ?? "0.0.0-local";
    });

    /// <summary>
    /// Full version string including build metadata (e.g., "1.4.2+20260607.a3f9c1").
    /// </summary>
    public static string Full => VersionLazy.Value;

    /// <summary>
    /// SemVer portion only (e.g., "1.4.2").
    /// </summary>
    public static string SemVer
    {
        get
        {
            var full = Full;
            var plusIndex = full.IndexOf('+');
            return plusIndex > 0 ? full[..plusIndex] : full;
        }
    }

    /// <summary>
    /// Build metadata portion (e.g., "20260607.a3f9c1"), or "local" if not stamped.
    /// </summary>
    public static string Build
    {
        get
        {
            var full = Full;
            var plusIndex = full.IndexOf('+');
            return plusIndex > 0 ? full[(plusIndex + 1)..] : "local";
        }
    }
}
