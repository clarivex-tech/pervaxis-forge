using FluentAssertions;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Naming;
using Pervaxis.Forge.Engine.Templating;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Templating;

public class ScribanTemplateEngineTests
{
    private readonly ScribanTemplateEngine engine = new();

    [Fact]
    public void Render_ReplacesModelValues()
    {
        var model = new TemplateModel
        {
            Manifest = new ForgeManifest
            {
                Product = "Pervaxis Forge",
                VerticalSlug = "clarivolt",
                ServiceName = "intake-service",
                ServiceType = ServiceType.RestApi,
                ComponentPrefix = "clv",
                CloudProvider = "AWS",
            },
            DerivedNames = new DerivedNames
            {
                DotNetNamespace = "Pervaxis.Forge.Intake",
                DotNetClassName = "IntakeService",
                AngularShellComponentName = "IntakeShellComponent",
                AngularMfeComponentName = "IntakeComponent",
                AngularShellRoutePath = "intake",
                AngularMfeRoutePath = "intake",
            },
        };

        var result = engine.Render("Product: {{ Product }}\nService: {{ DotNetClassName }}", model);

        result.Should().Contain("Product: Pervaxis Forge");
        result.Should().Contain("Service: IntakeService");
    }

    [Fact]
    public void Render_ThrowsOnInvalidTemplate()
    {
        var model = new TemplateModel
        {
            Manifest = new ForgeManifest
            {
                Product = "Pervaxis Forge",
                VerticalSlug = "clarivolt",
                ServiceName = "intake-service",
                ServiceType = ServiceType.RestApi,
                ComponentPrefix = "clv",
                CloudProvider = "AWS",
            },
            DerivedNames = new DerivedNames
            {
                DotNetNamespace = "Pervaxis.Forge.Intake",
                DotNetClassName = "IntakeService",
                AngularShellComponentName = "IntakeShellComponent",
                AngularMfeComponentName = "IntakeComponent",
                AngularShellRoutePath = "intake",
                AngularMfeRoutePath = "intake",
            },
        };

        Action action = () => engine.Render("{{ 1 + }}", model);

        action.Should().Throw<InvalidOperationException>();
    }
}
