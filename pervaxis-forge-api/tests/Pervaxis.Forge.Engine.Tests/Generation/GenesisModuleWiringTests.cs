/*
 ************************************************************************
 * Copyright (C) 2026 Clarivex Technologies Private Limited
 * All Rights Reserved.
 *
 * NOTICE: All intellectual and technical concepts contained
 * herein are proprietary to Clarivex Technologies Private Limited
 * and may be covered by Indian and Foreign Patents,
 * patents in process, and are protected by trade secret or
 * copyright law. Dissemination of this information or reproduction
 * of this material is strictly forbidden unless prior written
 * permission is obtained from Clarivex Technologies Private Limited.
 *
 * Product:   Pervaxis Platform
 * Website:   https://clarivex.tech
 ************************************************************************
 */
using System.IO.Compression;
using System.Text.Json;
using FluentAssertions;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Modules;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

/// <summary>
/// Each Genesis module is registered with configuration in the form its package reads, and its settings are written
/// where it looks. A mismatch silently ignores every setting of the module.
/// </summary>
public class GenesisModuleWiringTests
{
    // The modules whose Genesis package reads "Genesis:<Name>" from the configuration it is given (checked against
    // the Genesis 3.4 sources). All other modules bind the section they are given.
    private static readonly string[] ReadsGenesisSection = ["Idempotency", "OData", "TransactionalLogging", "FeatureFlags", "Sanitization"];

    private static ForgeManifest Manifest(IReadOnlyList<string> modules, bool database = true) => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "billing-service",
        ServiceType = ServiceType.RestApi,
        ComponentPrefix = "CLV",
        CloudProvider = "AWS",
        Database = database ? new DatabaseConfig { Engine = "postgresql", Name = "billing" } : null,
        GenesisModules = modules.ToList(),
    };

    private static async Task<Dictionary<string, string>> GenerateAsync(ForgeManifest manifest)
    {
        var bytes = await new PrintGenerator().GenerateAsync(manifest, "AWS");
        using var archive = new ZipArchive(new MemoryStream(bytes), ZipArchiveMode.Read);
        var files = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in archive.Entries.Where(e => !string.IsNullOrEmpty(e.Name)))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName.Replace('\\', '/')] = reader.ReadToEnd();
        }

        return files;
    }

    private static string Single(Dictionary<string, string> files, string suffix) =>
        files.Single(f => f.Key.EndsWith(suffix, StringComparison.Ordinal)).Value;

    [Fact]
    public void TheModuleRegistry_RecordsWhichModulesReadTheGenesisSection()
    {
        GenesisModules.GetAll().Where(m => m.UsesGenesisSection).Select(m => m.DisplayName)
            .Should().BeEquivalentTo(ReadsGenesisSection);
        GenesisModules.GetConfigurationKey("odata").Should().Be("Genesis:OData");
        GenesisModules.GetConfigurationKey("caching").Should().Be("Caching");
    }

    [Fact]
    public async Task EveryModule_IsRegisteredWithConfigurationInTheFormItsPackageReads()
    {
        var all = GenesisModules.GetAllNames();
        var program = Single(await GenerateAsync(Manifest(all)), "/Program.cs");

        foreach (var module in GenesisModules.GetAll())
        {
            var call = $"builder.Services.AddGenesis{module.DisplayName}(";
            program.Should().Contain(call);

            var registration = program[program.IndexOf(call, StringComparison.Ordinal)..];
            registration = registration[..registration.IndexOf(';', StringComparison.Ordinal)];

            if (module.UsesGenesisSection)
                registration.Should().Be($"builder.Services.AddGenesis{module.DisplayName}(builder.Configuration)", module.DisplayName);
            else
                registration.Should().Contain($"GetSection(\"{module.DisplayName}\")", module.DisplayName);
        }
    }

    [Fact]
    public async Task Appsettings_IsValidJson_AndPutsEachModuleWhereItIsRead()
    {
        var files = await GenerateAsync(Manifest(GenesisModules.GetAllNames()));

        using var json = JsonDocument.Parse(Single(files, "/appsettings.json"));
        var root = json.RootElement;
        var genesis = root.GetProperty("Genesis");

        foreach (var module in GenesisModules.GetAll())
        {
            if (module.UsesGenesisSection)
            {
                genesis.TryGetProperty(module.DisplayName, out _).Should().BeTrue(module.DisplayName);
                root.TryGetProperty(module.DisplayName, out _).Should().BeFalse($"{module.DisplayName} is read from Genesis:{module.DisplayName}");
            }
            else
            {
                root.TryGetProperty(module.DisplayName, out _).Should().BeTrue(module.DisplayName);
            }
        }

        genesis.GetProperty("OData").GetProperty("MaxTop").GetInt32().Should().Be(100);
        genesis.GetProperty("Sanitization").GetProperty("EnableMiddleware").GetBoolean().Should().BeFalse("it changes every field of every request, so it is opt-in");
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Appsettings_StaysValidJson_ForEveryModuleCombination(bool database)
    {
        string[][] combinations = [[], ["Caching"], ["OData"], ["Caching", "OData"], ["OData", "Sanitization"], ["Sanitization", "Caching", "FeatureFlags"]];
        foreach (var modules in combinations)
        {
            var appsettings = Single(await GenerateAsync(Manifest(modules, database)), "/appsettings.json");

            var parse = () => JsonDocument.Parse(appsettings).Dispose();
            parse.Should().NotThrow(string.Join(",", modules));
        }
    }

    [Fact]
    public async Task Sanitization_CapsJsonBodies_BeforeTheSanitizationMiddleware()
    {
        var files = await GenerateAsync(Manifest(["Sanitization"]));
        var program = Single(files, "/Program.cs");
        program.Should().Contain("app.UseGenesisSanitization();").And.Contain("app.UseMiddleware<JsonBodyLimitMiddleware>();");
        Single(files, "/Infrastructure/Middleware/JsonBodyLimitMiddleware.cs").Should().Contain("MaxInputLength");
        program.IndexOf("app.UseMiddleware<JsonBodyLimitMiddleware>();", StringComparison.Ordinal)
            .Should().BeLessThan(program.IndexOf("app.UseGenesisSanitization();", StringComparison.Ordinal), "the limit must run first");
        program.IndexOf("app.UseGenesisSanitization();", StringComparison.Ordinal)
            .Should().BeLessThan(program.IndexOf("app.MapControllers()", StringComparison.Ordinal));
    }

    [Fact]
    public async Task OData_RegistersThePagingGuard_AndShipsItsTests()
    {
        var files = await GenerateAsync(Manifest(["OData"]));

        Single(files, "/Program.cs").Should().Contain("builder.Services.AddODataPagingGuard();");
        Single(files, "/Infrastructure/Validation/ODataPagingGuardFilter.cs").Should().Contain("EffectiveTop < 0");
        Single(files, "/GenesisModuleTests.cs").Should().Contain("class ODataTests").And.NotContain("class SanitizationTests");
    }

    [Fact]
    public async Task WithoutODataOrSanitization_NoneOfTheirFilesAreGenerated()
    {
        var files = await GenerateAsync(Manifest(["Caching"]));

        Single(files, "/Program.cs").Should().NotContain("AddODataPagingGuard").And.NotContain("UseGenesisSanitization");
        files.Keys.Should().NotContain(k => k.EndsWith("/GenesisModuleTests.cs", StringComparison.Ordinal)
            || k.EndsWith("/ODataPagingGuardFilter.cs", StringComparison.Ordinal)
            || k.EndsWith("/JsonBodyLimitMiddleware.cs", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WithoutSanitization_ThereIsNoSanitizationMiddleware()
    {
        var program = Single(await GenerateAsync(Manifest(["OData"])), "/Program.cs");

        program.Should().NotContain("UseGenesisSanitization").And.NotContain("JsonBodyLimitMiddleware");
    }
}
