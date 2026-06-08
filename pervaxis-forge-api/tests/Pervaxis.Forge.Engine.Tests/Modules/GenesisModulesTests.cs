using FluentAssertions;
using Pervaxis.Forge.Engine.Modules;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Modules;

public class GenesisModulesTests
{
    [Fact]
    public void GetAll_ReturnsEightCloudAgnosticModules()
    {
        var modules = GenesisModules.GetAll();

        modules.Should().HaveCount(12);
        modules.Select(module => module.DisplayName).Should().BeEquivalentTo(new[]
        {
            "Caching",
            "Messaging",
            "FileStorage",
            "Search",
            "Notifications",
            "Workflow",
            "AIAssistance",
            "Reporting",
            "Idempotency",
            "OData",
            "TransactionalLogging",
            "FeatureFlags",
        });
    }

    [Theory]
    [InlineData("Caching", "AWS", "Pervaxis.Genesis.Caching.AWS", "AddGenesisCaching")]
    [InlineData("FileStorage", "Azure", "Pervaxis.Genesis.FileStorage.Azure", "AddGenesisFileStorage")]
    [InlineData("OData", "AWS", "Pervaxis.Genesis.OData", "AddGenesisOData")]
    public void PackageAndDiNames_AreDerivedCorrectly(string moduleName, string cloudProvider, string expectedPackage, string expectedDi)
    {
        GenesisModules.GetPackageName(moduleName, cloudProvider).Should().Be(expectedPackage);
        GenesisModules.GetDiExtensionName(moduleName, cloudProvider).Should().Be(expectedDi);
    }

    [Fact]
    public void GetById_IsCaseInsensitive()
    {
        GenesisModules.GetById("CACHING")!.DisplayName.Should().Be("Caching");
    }
}
