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

public sealed class AngularDomainPortabilityGenerationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task EmitsPlatformOwnedInfrastructureAndPortableDomainTooling(bool combined)
    {
        var manifest = new ForgeManifest
        {
            Product = "portability", VerticalSlug = "verify", ServiceName = "domain-host",
            ComponentPrefix = "dh", ServiceType = ServiceType.Monolithic, CloudProvider = "AWS",
            UiTargets = combined ? ["web", "mobile"] : [], AngularAuth = TestTenancy.AngularAuth,
        };
        using var zip = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        var entries = zip.Entries.ToDictionary(entry => entry.FullName, StringComparer.Ordinal);
        string Read(string suffix)
        {
            var matches = entries.Where(pair => ("/" + pair.Key).EndsWith(suffix, StringComparison.Ordinal)).ToArray();
            if (matches.Length != 1)
                throw new InvalidOperationException($"Expected one entry ending in '{suffix}', found {matches.Length}: {string.Join(", ", entries.Keys.Where(key => key.Contains("/domains/workspace/", StringComparison.Ordinal)))}");
            using var reader = new StreamReader(matches[0].Value.Open());
            return reader.ReadToEnd();
        }
        Assert.DoesNotContain(entries.Keys, key => key.Contains("/src/app/core/", StringComparison.Ordinal)
            || key.Contains("/src/app/features/", StringComparison.Ordinal) || key.Contains("/src/app/shared/", StringComparison.Ordinal));
        var workspaceFiles = entries.Keys.Where(key => key.Contains("/src/app/domains/workspace/", StringComparison.Ordinal)).ToArray();
        Assert.Equal(!combined, workspaceFiles.Length > 0);
        Assert.Equal(!combined, workspaceFiles.Any(key => key.EndsWith("/tasks/pages/my-tasks/my-tasks.component.html", StringComparison.Ordinal)));
        Assert.Contains("./platform/shell/app.component", Read("/app/app.component.ts"));
        Assert.Contains("./platform/platform.config", Read("/app/app.config.ts"));
        Assert.Contains("./platform/platform.routes", Read("/app/app.routes.ts"));
        Assert.Contains("UI_CONTRACT_MAJOR = 1", Read("/platform/contracts/v1/index.ts"));
        Assert.Contains("DomainHttpResponseV1", Read("/platform/contracts/v1/index.ts"));
        Assert.Contains("identityId", Read("/platform/contracts/v1/index.ts"));
        Assert.Contains("navigationPath", Read("/platform/contracts/v1/index.ts"));
        Assert.Contains("getResponse<T>", Read("/platform/core/services/api.service.ts"));
        Assert.Contains("deleteResponse: <T>", Read("/platform/domains/domain-runtime.ts"));
        Assert.Contains("...createDomainRoutes(GENERATED_DOMAINS)", Read("/platform/platform.routes.ts"));
        Assert.Contains("...domainProviders(GENERATED_DOMAINS)", Read("/platform/platform.config.ts"));
        Assert.Contains("readonly UiDomainRegistrationV1[]", Read("/.generated/domain-registry.ts"));
        Assert.Contains("@forge/ui-contracts/v1", Read("tsconfig.json"));
        Assert.Contains("scripts/domain-tool.mjs && ng build", Read("package.json"));
        Assert.Contains("ts.resolveModuleName", Read("/scripts/domain-tool.mjs"));
        Assert.Contains("navigationPath must be a relative route", Read("/scripts/domain-tool.mjs"));
        Assert.Contains("platformBefore", Read("/scripts/verify-copy.mjs"));
        Assert.Contains("npm run domains:prepare", Read("/.github/workflows/ci.yml"));
        Assert.Contains("npm run test:architecture", Read("/.github/workflows/ci.yml"));
        foreach (System.Text.RegularExpressions.Match route in System.Text.RegularExpressions.Regex.Matches(
            Read("/platform/platform.routes.ts"), "path: '([^']+)'"))
        {
            var root = route.Groups[1].Value.Split('/')[0];
            if (root != "**") Assert.Contains("'" + root + "'", Read("/scripts/domain-tool.mjs"));
        }
        Assert.Contains("registerUntilDestroyed", Read("/DomainExample/services/work-items.service.ts"));
        Assert.Contains("epoch", Read("/DomainExample/services/work-items.service.ts"));
        Assert.DoesNotContain("domain-host", Read("/DomainExample/routes.ts"));
        using var host = JsonDocument.Parse(Read("ui-platform.json"));
        Assert.Equal(1, host.RootElement.GetProperty("contractVersion").GetInt32());
        Assert.Equal(combined ? new[] { "web", "mobile" } : new[] { "web" },
            host.RootElement.GetProperty("targets").EnumerateArray().Select(target => target.GetString()));
        if (!combined)
        {
            using var domain = JsonDocument.Parse(Read("/domains/workspace/domain.json"));
            Assert.Equal("cognito-tenant", domain.RootElement.GetProperty("security").GetString());
            Assert.Equal("workspace", domain.RootElement.GetProperty("id").GetString());
            Assert.Equal("tasks", domain.RootElement.GetProperty("navigationPath").GetString());
            Assert.Equal("web", domain.RootElement.GetProperty("supportedTargets").EnumerateArray().Single().GetString());
            Assert.Contains("registerUntilDestroyed", Read("/domains/workspace/tasks/state/workspace-task.state.ts"));
            Assert.Contains("/v1/workspace/tasks", Read("/domains/workspace/domain.config.json"));
            Assert.Contains("If-Match", Read("/domains/workspace/tasks/api/workspace-task.api.ts"));
            Assert.Contains("[textContent]", Read("/domains/workspace/tasks/pages/my-tasks/my-tasks.component.html"));
            Assert.Contains("DatePipe", Read("/domains/workspace/tasks/pages/my-tasks/my-tasks.component.ts"));
        }
    }
}
