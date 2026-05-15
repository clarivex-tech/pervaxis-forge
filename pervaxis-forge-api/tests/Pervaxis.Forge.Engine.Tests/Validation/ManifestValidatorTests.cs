using FluentAssertions;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Modules;
using Pervaxis.Forge.Engine.Validation;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Validation;

public class ManifestValidatorTests
{
    private readonly ManifestValidator validator = new();

    [Fact]
    public void Validate_ReturnsValidForCompleteManifest()
    {
        var manifest = CreateValidManifest();

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("NotKebab")]
    [InlineData("bad_value")]
    [InlineData("bad value")]
    public void Validate_RejectsInvalidVerticalSlug(string verticalSlug)
    {
        var manifest = CreateValidManifest() with { VerticalSlug = verticalSlug };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(error => error.Contains("VerticalSlug"));
    }

    [Fact]
    public void Validate_RequiresDotNetServicesToEndWithService()
    {
        var manifest = CreateValidManifest() with { ServiceName = "billing-api", ServiceType = ServiceType.RestApi };

        var result = validator.Validate(manifest);

        result.Errors.Should().Contain(error => error.Contains("-service"));
    }

    [Fact]
    public void Validate_RejectsServiceSuffixForAngularMfe()
    {
        var manifest = CreateValidManifest() with { ServiceType = ServiceType.AngularMfe, ServiceName = "billing-service" };

        var result = validator.Validate(manifest);

        result.Errors.Should().Contain(error => error.Contains("Angular MFE") && error.Contains("-service"));
    }

    [Fact]
    public void Validate_ReportsMissingRequiredFields()
    {
        var manifest = new ForgeManifest
        {
            Product = string.Empty,
            VerticalSlug = string.Empty,
            ServiceName = string.Empty,
            ServiceType = ServiceType.RestApi,
            ComponentPrefix = string.Empty,
            CloudProvider = string.Empty,
        };

        var result = validator.Validate(manifest);

        result.Errors.Should().Contain(error => error.Contains("Product"));
        result.Errors.Should().Contain(error => error.Contains("ComponentPrefix"));
        result.Errors.Should().Contain(error => error.Contains("CloudProvider"));
    }

    [Fact]
    public void GenesisModules_AreComplete()
    {
        var modules = GenesisModules.GetAll();

        modules.Should().HaveCount(8);
        modules.Select(module => module.DisplayName).Should().Contain(new[]
        {
            "Caching",
            "Messaging",
            "FileStorage",
            "Search",
            "Notifications",
            "Workflow",
            "AIAssistance",
            "Reporting",
        });
    }

    [Fact]
    public void CanvasModules_AreComplete()
    {
        var modules = CanvasModules.GetAll();

        modules.Should().HaveCount(14);
        modules.Select(module => module.DisplayName).Should().Contain("Workspace");
    }

    private static ForgeManifest CreateValidManifest() => new()
    {
        Product = "Pervaxis Forge",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "clv",
        CloudProvider = "AWS",
        Database = new DatabaseConfig { Engine = "postgresql" },
        Queue = new QueueConfig { Provider = "SQS", Enabled = true },
        Api = new ApiConfig { BasePath = "/api/v1/billing", SwaggerEnabled = true, CorsEnabled = true },
        Angular = new AngularConfig { WorkspaceName = "workspace", ProjectName = "billing-shell", RoutePrefix = "billing" },
    };
}
