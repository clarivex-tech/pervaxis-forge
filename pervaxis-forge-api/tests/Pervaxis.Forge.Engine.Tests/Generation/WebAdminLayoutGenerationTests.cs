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
        var shell = await Read("/shell/app.component.html");
        Assert.Equal(!combined, shell.Contains("<p-drawer", StringComparison.Ordinal));
        Assert.Equal(!combined, zip.Entries.Any(e => e.FullName.EndsWith("/theme.service.ts", StringComparison.Ordinal)));
        Assert.Equal(!combined, shell.Contains("Skip to main content", StringComparison.Ordinal));
        var styles = await Read("/styles.scss");
        Assert.Equal(!combined, styles.Contains("--color-primary: var(--p-primary-color)", StringComparison.Ordinal));
        Assert.Equal(!combined, (await Read("/platform.config.ts")).Contains("ripple: true", StringComparison.Ordinal));
        var admin = await Read("/administration.component.html");
        Assert.Equal(!combined, admin.Contains("pInputText", StringComparison.Ordinal));
        Assert.Contains("permissionGuard('PlatformAdmin')", await Read("/platform.routes.ts"));
        Assert.Contains("SessionStateRegistry", await Read("/administration.component.ts"));
        Assert.Contains("epoch === this.epoch", await Read("/administration.component.ts"));
        var packages = await Read("package.json");
        Assert.Contains("primeng", packages);
        Assert.DoesNotContain("ng-matero", packages);
        Assert.DoesNotContain("@angular/material", packages);
        Assert.DoesNotContain("{{", shell);
    }
    [Fact]
    public async Task RenderedShell_HidesPublicChromeAndKeepsSettingsInAuthenticatedShell()
    {
        var manifest = new ForgeManifest
        {
            Product = "verify", VerticalSlug = "verify", ServiceName = "admin-web",
            ComponentPrefix = "vfy", ServiceType = ServiceType.Monolithic, CloudProvider = "AWS",
            AngularAuth = TestTenancy.AngularAuth,
            UiTargets = ["web"],
        };
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));

        var shell = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith("/shell/app.component.html", StringComparison.Ordinal)).Open()).ReadToEnd();
        var component = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith("/shell/app.component.ts", StringComparison.Ordinal)).Open()).ReadToEnd();

        Assert.Contains("!auth.isAuthenticated()", shell);
        Assert.Contains("@if (auth.isAuthenticated())", shell);
        Assert.DoesNotContain("tenant.currentTenantId", shell);
        Assert.DoesNotContain("app-header__tenant", shell);
        Assert.Contains("routerLink: '/settings'", component);
        var workspaceConfig = new StreamReader(zip.Entries.Single(e =>
            e.FullName.EndsWith("/domains/workspace/domain.config.json", StringComparison.Ordinal)).Open()).ReadToEnd();
        Assert.Contains("\"apiPath\": \"/api/v1/workspace/tasks\"", workspaceConfig);
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

    [Fact]
    public async Task WebAdminThemeUsesCentralSemanticIndigoSlateTokens()
    {
        var manifest = new ForgeManifest
        {
            Product = "verify", VerticalSlug = "verify", ServiceName = "admin-web",
            ComponentPrefix = "vfy", ServiceType = ServiceType.Monolithic, CloudProvider = "AWS",
            AngularAuth = TestTenancy.AngularAuth,
        };
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        async Task<string> Read(string suffix)
        {
            using var reader = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith(suffix, StringComparison.Ordinal)).Open());
            return await reader.ReadToEndAsync();
        }

        var preset = await Read("/platform.config.ts");
        Assert.Contains("color: '{indigo.600}'", preset);
        Assert.Contains("hoverColor: '{indigo.700}'", preset);
        Assert.Contains("color: '{indigo.300}'", preset);
        Assert.Contains("contrastColor: '{slate.900}'", preset);
        Assert.Contains("width: '3px'", preset);

        var styles = await Read("/styles.scss");
        Assert.Contains("--color-bg: var(--p-slate-50)", styles);
        Assert.Contains("--color-bg: var(--p-slate-900)", styles);
        Assert.Contains("--color-header-bg: var(--p-indigo-950)", styles);
        Assert.Contains("--color-header-focus: var(--p-indigo-200)", styles);
        Assert.Contains("--color-sidebar-bg: var(--p-indigo-50)", styles);
        Assert.Contains("--color-sidebar-bg: var(--p-indigo-900)", styles);
        Assert.Contains("--p-drawer-background: var(--color-sidebar-bg)", styles);
        Assert.Contains("--color-success-bg: var(--p-green-950)", styles);
        Assert.Contains("--color-danger-bg: var(--p-red-950)", styles);

        Assert.Contains(".app-header__brand, .app-header__user { color: var(--color-header-text); }", styles);
        Assert.Contains(".sidebar-item.active", styles);

        var workspace = await Read("/my-tasks.component.scss");
        Assert.Contains("var(--color-success-bg)", workspace);
        Assert.Contains("var(--color-danger-border)", workspace);
        Assert.DoesNotContain("#", workspace);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WorkspaceCrudaReferenceStaysInsideDomainAndWebOnly(bool combined)
    {
        var manifest = new ForgeManifest
        {
            Product = "verify", VerticalSlug = "verify", ServiceName = "admin-web",
            ComponentPrefix = "vfy", ServiceType = ServiceType.Monolithic, CloudProvider = "AWS",
            AngularAuth = TestTenancy.AngularAuth,
            UiTargets = combined ? ["web", "mobile"] : ["web"],
        };
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        const string root = "/domains/workspace/workspaces/";
        var files = zip.Entries.Where(e => e.FullName.Contains(root, StringComparison.Ordinal)).ToList();
        var packages = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith("package.json", StringComparison.Ordinal)).Open()).ReadToEnd();
        if (combined)
        {
            Assert.Empty(files);
            Assert.DoesNotContain("ag-grid", packages);
            return;
        }

        string Read(string suffix) => new StreamReader(files.Single(e => e.FullName.EndsWith(suffix, StringComparison.Ordinal)).Open()).ReadToEnd();
        foreach (var required in new[]
        {
            "components/workspace-grid/workspace-grid.component.ts", "components/workspace-drawer/workspace-drawer.component.ts",
            "components/workspace-form/workspace-form.component.ts", "components/workspace-detail/workspace-detail.component.ts",
            "components/workspace-delete-dialog/workspace-delete-dialog.component.ts", "pages/workspace-records/workspace-records.component.ts",
            "state/workspace-records.state.ts", "sources/mock-workspace-records.data.ts", "sources/workspace-records.providers.ts",
        })
        {
            Assert.Contains(files, e => e.FullName.EndsWith(required, StringComparison.Ordinal));
        }

        // AG Grid Community and PrimeNG only; no Enterprise, Ng-Matero or Canvas.
        Assert.Contains("\"ag-grid-community\"", packages);
        Assert.Contains("\"ag-grid-angular\"", packages);
        Assert.DoesNotContain("ag-grid-enterprise", packages);
        Assert.DoesNotContain("ng-matero", packages);

        // Domain-owned: declared requirements, explicit mock mode, and no host/platform imports.
        var domain = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith("/domains/workspace/domain.json", StringComparison.Ordinal)).Open()).ReadToEnd();
        Assert.Contains("\"ag-grid-community\"", domain);
        Assert.Contains("\"workspaceRecordsMode\"", domain);
        var config = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith("/domains/workspace/domain.config.json", StringComparison.Ordinal)).Open()).ReadToEnd();
        Assert.Contains("\"workspaceRecordsMode\": \"mock\"", config);
        Assert.Contains("\"apiPath\": \"/api/v1/workspace/tasks\"", config);
        foreach (var file in files.Where(e => e.FullName.EndsWith(".ts", StringComparison.Ordinal)))
        {
            var text = new StreamReader(file.Open()).ReadToEnd();
            Assert.DoesNotContain("{{", text);
            Assert.DoesNotContain("ag-grid-enterprise", text);
            Assert.DoesNotContain("/platform/", text);
            Assert.DoesNotContain("@forge/ui-contracts/v2", text);
        }
        foreach (var file in files.Where(e => e.FullName.EndsWith(".html", StringComparison.Ordinal)))
        {
            Assert.DoesNotContain("{{", new StreamReader(file.Open()).ReadToEnd());
        }

        // Mock data is selected only by the explicit option and never reached from the live Tasks feature.
        var providers = Read("sources/workspace-records.providers.ts");
        Assert.Contains("=== WORKSPACE_RECORDS_MOCK_MODE", providers);
        Assert.Contains("UnconfiguredWorkspaceRecordsData", providers);
        foreach (var file in zip.Entries.Where(e => e.FullName.Contains("/domains/workspace/tasks/", StringComparison.Ordinal)
                     && e.FullName.EndsWith(".ts", StringComparison.Ordinal)))
        {
            var text = new StreamReader(file.Open()).ReadToEnd();
            Assert.DoesNotContain("Mock", text);
            Assert.DoesNotContain("workspaces/", text);
        }
        var routes = new StreamReader(zip.Entries.Single(e => e.FullName.EndsWith("/domains/workspace/routes.ts", StringComparison.Ordinal)).Open()).ReadToEnd();
        Assert.Contains("providers: [WorkspaceTaskApi, WorkspaceTaskState]", routes);
        Assert.Contains("path: 'workspaces'", routes);

        // Accessibility and theming contract of the screen.
        var grid = Read("workspace-grid/workspace-grid.component.ts");
        Assert.Contains("AllCommunityModule", grid);
        Assert.Contains("var(--color-surface)", grid);
        var page = Read("workspace-records/workspace-records.component.html");
        Assert.Contains("role=\"search\"", page);
        Assert.Contains("aria-label=\"Breadcrumb\"", page);
        Assert.Contains("ERR-WS", Read("sources/mock-workspace-records.data.ts"));
    }}
