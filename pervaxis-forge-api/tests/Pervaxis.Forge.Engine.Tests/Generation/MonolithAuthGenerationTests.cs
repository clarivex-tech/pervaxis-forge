using System.IO.Compression;
using System.Text;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public class MonolithAuthGenerationTests
{
    private static ForgeManifest Manifest(string[] targets) => new()
    {
        Product = "clarivolt",
        VerticalSlug = "clarivolt",
        ServiceName = "claims-web",
        ServiceType = ServiceType.Monolithic,
        AngularAuth = TestTenancy.AngularAuth,
        ComponentPrefix = "clv",
        CloudProvider = "AWS",
        UiTargets = targets.ToList(),
    };

    [Fact]
    public async Task WebPrint_ShouldIncludeCognitoAuth()
    {
        // Arrange
        var generator = new PrintGenerator();
        var manifest = Manifest(["web"]);

        // Act
        var zip = await generator.GenerateAsync(manifest, "AWS");

        // Assert
        using var ms = new MemoryStream(zip);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        var entries = archive.Entries.Select(e => e.FullName).ToHashSet();

        // Auth files must exist
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/auth.models.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/auth.service.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/cognito-client.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/auth-sync.service.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/session-state.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/auth.service.spec.ts", entries);

        // Config files
        Assert.Contains("apps/claims-web/src/app/platform/core/config/app-config.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/config/app-config.service.ts", entries);
        Assert.Contains("apps/claims-web/src/assets/app-config.example.json", entries);

        // Cognito configuration is populated at generation.
        Assert.Contains("apps/claims-web/src/assets/app-config.json", entries);

        // Guards and interceptors
        Assert.Contains("apps/claims-web/src/app/platform/core/guards/auth.guard.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/interceptors/auth.interceptor.ts", entries);

        // Auth feature
        Assert.Contains("apps/claims-web/src/app/platform/features/auth/auth-callback.component.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/features/login/login.component.ts", entries);

        // Documentation
        Assert.Contains("AUTHENTICATION.md", entries);

        // package.json should have oauth4webapi
        var packageJsonEntry = archive.GetEntry("package.json");
        Assert.NotNull(packageJsonEntry);
        using var packageStream = packageJsonEntry!.Open();
        using var reader = new StreamReader(packageStream, Encoding.UTF8);
        var packageJson = await reader.ReadToEndAsync();
        Assert.Contains("\"oauth4webapi\": \"3.8.8\"", packageJson);

        // Should NOT contain mock token service
        Assert.DoesNotContain("mock-access-token", packageJson);

        // .gitignore should have app-config.json
        var gitignoreEntry = archive.GetEntry(".gitignore");
        Assert.NotNull(gitignoreEntry);
        using var gitignoreStream = gitignoreEntry!.Open();
        using var gitignoreReader = new StreamReader(gitignoreStream, Encoding.UTF8);
        var gitignore = await gitignoreReader.ReadToEndAsync();
        Assert.DoesNotContain("apps/*/src/assets/app-config.json", gitignore);
    }

    [Fact]
    public async Task MobilePrint_ShouldIncludeCognitoAuthFiles()
    {
        // Arrange
        var generator = new PrintGenerator();
        var manifest = Manifest(["web", "mobile"]);

        // Act
        var zip = await generator.GenerateAsync(manifest, "AWS");

        // Assert
        using var ms = new MemoryStream(zip);
        using var archive = new ZipArchive(ms, ZipArchiveMode.Read);
        var entries = archive.Entries.Select(e => e.FullName).ToHashSet();

        // Auth files must exist (same as web - Cognito works via browser on mobile)
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/auth.models.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/auth.service.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/cognito-client.ts", entries);
        Assert.Contains("apps/claims-web/src/app/platform/core/auth/auth-sync.service.ts", entries);

        // Login component uses AuthService for both web and mobile
        var loginEntry = archive.GetEntry("apps/claims-web/src/app/platform/features/login/login.component.ts");
        Assert.NotNull(loginEntry);
        using var loginStream = loginEntry!.Open();
        using var loginReader = new StreamReader(loginStream, Encoding.UTF8);
        var loginContent = await loginReader.ReadToEndAsync();
        Assert.Contains("AuthService", loginContent);
    }

    private static AngularAuthConfig Cognito() => new()
    {
        Provider = "Cognito", UserPoolId = "us-east-1_Example", ClientId = "exampleclient123",
        Domain = "https://example.auth.us-east-1.amazoncognito.com",
        Scopes = ["openid", "email", "phone"],
        RedirectUri = "http://localhost:4200/auth/callback",
        PostLogoutRedirectUri = "http://localhost:4200/login"
    };

    [Fact]
    public async Task SelectedCognito_EmitsConfiguredRuntimeJson()
    {
        var manifest = Manifest(["web"]) with { AngularAuth = Cognito() };
        using var archive = new ZipArchive(new MemoryStream(await new PrintGenerator().GenerateAsync(manifest, "AWS")));
        var content = await Read(archive, "apps/claims-web/src/assets/app-config.json");
        using var config = System.Text.Json.JsonDocument.Parse(content);
        Assert.Equal("MultiTenant", config.RootElement.GetProperty("tenancy").GetProperty("mode").GetString());
        Assert.Equal("tenant_id", config.RootElement.GetProperty("tenancy").GetProperty("claimType").GetString());
        var auth = config.RootElement.GetProperty("auth");
        Assert.Equal(Cognito().Domain, auth.GetProperty("cognitoDomain").GetString());
        Assert.Equal("https://cognito-idp.us-east-1.amazonaws.com/us-east-1_Example", auth.GetProperty("issuer").GetString());
        Assert.Equal(Cognito().ClientId, auth.GetProperty("clientId").GetString());
        Assert.Equal(Cognito().RedirectUri, auth.GetProperty("redirectUri").GetString());
        Assert.Equal(Cognito().PostLogoutRedirectUri, auth.GetProperty("postLogoutRedirectUri").GetString());
        Assert.Contains("phone", auth.GetProperty("scopes").EnumerateArray().Select(v => v.GetString()));
        Assert.DoesNotContain("apps/*/src/assets/app-config.json", await Read(archive, ".gitignore"));
        Assert.Contains("cognito-client.ts", string.Join(" ", archive.Entries.Select(e => e.FullName)));
    }

    [Fact]
    public async Task None_IsRejectedForTenantProtectedMonolith()
    {
        var manifest = Manifest(["web"]) with { AngularAuth = new() { Provider = "None" } };
        await Assert.ThrowsAsync<InvalidOperationException>(() => new PrintGenerator().GenerateAsync(manifest, "AWS"));
    }

    [Theory]
    [InlineData("UserPoolId")]
    [InlineData("ClientId")]
    [InlineData("Domain")]
    [InlineData("Scopes")]
    [InlineData("RedirectUri")]
    [InlineData("PostLogoutRedirectUri")]
    public void Cognito_RequiresEveryBrowserSetting(string field)
    {
        var auth = field switch
        {
            "UserPoolId" => Cognito() with { UserPoolId = null },
            "ClientId" => Cognito() with { ClientId = null },
            "Domain" => Cognito() with { Domain = "http://insecure.example.com" },
            "Scopes" => Cognito() with { Scopes = ["email"] },
            "RedirectUri" => Cognito() with { RedirectUri = "https://app.example.com/wrong" },
            _ => Cognito() with { PostLogoutRedirectUri = "https://other.example.com/login" }
        };
        var result = new Pervaxis.Forge.Engine.Validation.ManifestValidator().Validate(Manifest(["web"]) with { AngularAuth = auth });
        Assert.Contains(result.Errors, e => e.Contains("AngularAuth." + field));
    }

    private static async Task<string> Read(ZipArchive archive, string path)
    {
        using var reader = new StreamReader(archive.GetEntry(path)!.Open());
        return await reader.ReadToEndAsync();
    }
}
