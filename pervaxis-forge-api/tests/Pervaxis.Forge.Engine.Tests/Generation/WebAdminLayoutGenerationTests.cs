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
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public sealed class WebAdminLayoutGenerationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task SelectsWebLayoutWithoutChangingCombinedTarget(bool combined, bool useDefaultTargets)
    {
        var manifest = new ForgeManifest
        {
            Product = "verify", VerticalSlug = "verify", ServiceName = "admin-web",
            ComponentPrefix = "vfy", ServiceType = ServiceType.Monolithic, CloudProvider = "AWS",
            AngularAuth = TestTenancy.AngularAuth,
        };
        if (!useDefaultTargets) manifest = manifest with { UiTargets = combined ? ["web", "mobile"] : ["web"] };
        Assert.True(new Pervaxis.Forge.Engine.Validation.ManifestValidator().Validate(manifest).IsValid);
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        async Task<string> Read(string suffix)
        {
            using var reader = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith(suffix, StringComparison.Ordinal)).Open());
            return await reader.ReadToEndAsync();
        }
        using var metadata = JsonDocument.Parse(await Read("manifest.json"));
        Assert.Equal(combined ? new[] { "web", "mobile" } : new[] { "web" },
            metadata.RootElement.GetProperty("uiTargets").EnumerateArray().Select(target => target.GetString()));
        var shell = await Read("/app.component.html");
        Assert.Equal(!combined, shell.Contains("<p-drawer", StringComparison.Ordinal));
        Assert.Equal(!combined, zip.Entries.Any(e => e.FullName.EndsWith("/theme.service.ts", StringComparison.Ordinal)));
        Assert.Equal(!combined, shell.Contains("Skip to main content", StringComparison.Ordinal));
        var admin = await Read("/administration.component.html");
        Assert.Equal(!combined, admin.Contains("pInputText", StringComparison.Ordinal));
        Assert.Contains("permissionGuard('PlatformAdmin')", await Read("/app.routes.ts"));
        Assert.Contains("SessionStateRegistry", await Read("/administration.component.ts"));
        Assert.Contains("epoch === this.epoch", await Read("/administration.component.ts"));
        var packages = await Read("package.json");
        Assert.Contains("primeng", packages);
        Assert.DoesNotContain("ng-matero", packages);
        Assert.DoesNotContain("@angular/material", packages);
        Assert.DoesNotContain("{{", shell);
    }
    [Fact]
    public async Task DefaultAndExplicitWebRenderIdenticalApplicationSources()
    {
        var manifest = new ForgeManifest
        {
            Product = "verify", VerticalSlug = "verify", ServiceName = "admin-web",
            ComponentPrefix = "vfy", ServiceType = ServiceType.Monolithic, CloudProvider = "AWS",
            AngularAuth = TestTenancy.AngularAuth,
        };
        var model = TemplateModelBuilder.Build(manifest, "AWS");
        var generator = new FileGenerator();
        var defaults = await generator.GenerateAsync("Templates/angular-monolith", model);
        var explicitWeb = await generator.GenerateAsync("Templates/angular-monolith",
            model with { Manifest = manifest with { UiTargets = ["web"] } });
        Assert.Equal(explicitWeb.Select(file => (file.Path, file.Content)),
            defaults.Select(file => (file.Path, file.Content)));
    }

}
