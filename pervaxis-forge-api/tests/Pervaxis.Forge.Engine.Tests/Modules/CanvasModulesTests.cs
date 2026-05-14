using FluentAssertions;
using Pervaxis.Forge.Engine.Modules;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Modules;

public class CanvasModulesTests
{
    [Fact]
    public void GetAll_ReturnsFourteenModules()
    {
        var modules = CanvasModules.GetAll();

        modules.Should().HaveCount(14);
        modules.Select(module => module.DisplayName).Should().Contain(new[]
        {
            "Workspace",
            "Shell",
            "Layout",
            "Navigation",
            "Auth",
            "Settings",
            "Profile",
            "Dashboard",
            "Notifications",
            "Search",
            "Reports",
            "Analytics",
            "Admin",
            "Support",
        });
    }

    [Fact]
    public void GetById_IsCaseInsensitive()
    {
        CanvasModules.GetById("SHELL")!.DisplayName.Should().Be("Shell");
    }
}
