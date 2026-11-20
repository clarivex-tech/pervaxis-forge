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

public sealed class SettingsGenerationTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task SettingsBelongsToWebPlatformAndUsesExistingIdentityAndMeContract(bool combined, bool useDefaultTargets)
    {
        var manifest = new ForgeManifest
        {
            Product = "settings", VerticalSlug = "verify", ServiceName = "profile-web", ComponentPrefix = "prf",
            ServiceType = ServiceType.Monolithic, CloudProvider = "AWS", AngularAuth = TestTenancy.AngularAuth,
            UiTargets = useDefaultTargets ? [] : combined ? ["web", "mobile"] : ["web"],
        };
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        string Read(string suffix)
        {
            using var reader = new StreamReader(zip.Entries.Single(entry => ("/" + entry.FullName).EndsWith(suffix, StringComparison.Ordinal)).Open());
            return reader.ReadToEnd();
        }
        Assert.Equal(!combined, zip.Entries.Any(entry => entry.FullName.EndsWith("/features/settings/settings.component.ts", StringComparison.Ordinal)));
        Assert.Equal(!combined, Read("/platform/platform.routes.ts").Contains("path: 'settings'", StringComparison.Ordinal));
        Assert.Equal(!combined, Read("/platform/shell/app.component.html").Contains("user-account-menu", StringComparison.Ordinal));
        Assert.Contains("permissionGuard('PlatformAdmin')", Read("/platform/platform.routes.ts"));
        Assert.Contains("...createDomainRoutes(GENERATED_DOMAINS)", Read("/platform/platform.routes.ts"));
        Assert.DoesNotContain(zip.Entries, entry => entry.FullName.Contains("/src/app/domains/settings/", StringComparison.Ordinal));
        using var angular = JsonDocument.Parse(Read("angular.json"));
        var optimization = angular.RootElement.GetProperty("projects").GetProperty("profile-web")
            .GetProperty("architect").GetProperty("build").GetProperty("configurations").GetProperty("production").GetProperty("optimization");
        if (combined) { Assert.Equal(JsonValueKind.True, optimization.ValueKind); return; }
        Assert.False(optimization.GetProperty("styles").GetProperty("inlineCritical").GetBoolean());
        Assert.Contains("canActivate: [authGuard],", Read("/platform/platform.routes.ts"));
        var state = Read("/features/settings/my-profile-state.ts");
        Assert.Contains("get<unknown>('/api/v1/me')", state);
        Assert.Contains("context.subject !== subject", state);
        Assert.Contains("epoch === this.epoch", state);
        Assert.Contains("registerUntilDestroyed", state);
        Assert.DoesNotContain("auth.roles", state);
        Assert.DoesNotContain("GetUser", state);
        Assert.DoesNotContain("fetch(", state);
        var component = Read("/features/settings/settings.component.ts");
        Assert.Contains("this.auth.profile()", component);
        Assert.Contains("ThemeService", component);
        var html = Read("/features/settings/settings.component.html");
        Assert.Contains("Not provided", html);
        Assert.DoesNotContain("<input", html);
        Assert.DoesNotContain("password", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("aws.cognito.signin.user.admin", Read("/assets/app-config.json"));
    }
    [Fact]
    public async Task WebStillRequiresExistingCognitoConfiguration()
    {
        var manifest = new ForgeManifest
        {
            Product = "settings", VerticalSlug = "verify", ServiceName = "profile-public", ComponentPrefix = "prf",
            ServiceType = ServiceType.Monolithic, CloudProvider = "AWS", UiTargets = [],
        };
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new PrintGenerator().GenerateAsync(manifest, "AWS"));
        Assert.Contains("require AngularAuth with Cognito settings", error.Message);
    }
}
