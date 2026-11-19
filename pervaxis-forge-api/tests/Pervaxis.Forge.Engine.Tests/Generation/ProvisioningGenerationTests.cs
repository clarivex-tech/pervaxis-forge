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

public sealed class ProvisioningGenerationTests
{
    [Theory]
    [InlineData(ServiceType.RestApi)]
    [InlineData(ServiceType.Monolithic)]
    public async Task GeneratesConnectedAdministration(ServiceType type)
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt", VerticalSlug = "clarivolt", ServiceName = type == ServiceType.RestApi ? "onboarding-service" : "onboarding-web",
            ComponentPrefix = "clv", ServiceType = type, CloudProvider = "AWS",
            Database = type == ServiceType.RestApi ? new DatabaseConfig { Engine = "postgresql", Name = "onboarding" } : null,
            MultiTenancy = type == ServiceType.RestApi, InitialTenantId = TestTenancy.InitialTenantId,
            Cognito = type == ServiceType.RestApi ? TestTenancy.Cognito : null,
            AngularAuth = type == ServiceType.Monolithic ? TestTenancy.AngularAuth : null,
            UiTargets = ["web"],
        };
        using var archive = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        var files = new Dictionary<string, string>();
        foreach (var entry in archive.Entries.Where(entry => entry.Name.Length > 0))
        {
            using var reader = new StreamReader(entry.Open());
            files[entry.FullName] = await reader.ReadToEndAsync();
        }
        string File(string suffix) => files.Single(file => file.Key.EndsWith(suffix, StringComparison.Ordinal)).Value;
        if (type == ServiceType.RestApi)
        {
            using var config = JsonDocument.Parse(File("/appsettings.json"));
            var provisioning = config.RootElement.GetProperty("Forge").GetProperty("Provisioning");
            Assert.False(provisioning.GetProperty("Enabled").GetBoolean());
            Assert.Equal(TestTenancy.Cognito.UserPoolId, provisioning.GetProperty("UserPoolId").GetString());
            Assert.Contains("PlatformAdminPolicy", File("/TenantUsersController.cs"));
            Assert.Contains("[TenantExempt]", File("/TenantUsersController.cs"));
            Assert.Contains("CognitoUserProvisioningService", File("/Platform/Hosting/ForgeHost.cs"));
            Assert.Contains("AWSSDK.CognitoIdentityProvider", File("Directory.Packages.props"));
            Assert.Contains("Tenant reassignment is not supported", File("/CognitoUserProvisioningService.cs"));
            Assert.Contains("ProvisioningTests.cs", string.Join(",", files.Keys));
        }
        else
        {
            var routes = File("/app.routes.ts");
            Assert.Contains("path: 'admin/tenants'", routes);
            Assert.Contains("permissionGuard('PlatformAdmin')", routes);
            Assert.Contains("resend-invitation", File("/administration.service.ts"));
            Assert.Contains("[textContent]", File("/administration.component.html"));
            Assert.DoesNotContain("{{", File("/administration.component.html"));
            Assert.Contains("SessionStateRegistry", File("/administration.component.ts"));
        }
    }
}
