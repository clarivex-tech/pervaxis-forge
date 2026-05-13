namespace Pervaxis.Forge.Engine.Modules;

public sealed record CanvasModule(string Id, string DisplayName);

public static class CanvasModules
{
    private static readonly CanvasModule[] Modules =
    [
        new("workspace", "Workspace"),
        new("shell", "Shell"),
        new("layout", "Layout"),
        new("navigation", "Navigation"),
        new("auth", "Auth"),
        new("settings", "Settings"),
        new("profile", "Profile"),
        new("dashboard", "Dashboard"),
        new("notifications", "Notifications"),
        new("search", "Search"),
        new("reports", "Reports"),
        new("analytics", "Analytics"),
        new("admin", "Admin"),
        new("support", "Support"),
    ];

    public static IReadOnlyList<CanvasModule> GetAll() => Modules;

    public static CanvasModule? GetById(string id)
        => Modules.FirstOrDefault(module => string.Equals(module.Id, id, StringComparison.OrdinalIgnoreCase));

    public static IReadOnlyList<string> GetAllNames() => Modules.Select(module => module.DisplayName).ToArray();
}
