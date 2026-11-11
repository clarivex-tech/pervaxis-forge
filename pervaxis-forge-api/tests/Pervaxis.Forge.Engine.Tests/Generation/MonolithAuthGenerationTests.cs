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
        Assert.Contains("apps/claims-web/src/app/core/auth/auth.models.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/auth.service.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/cognito-client.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/auth-sync.service.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/session-state.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/auth.service.spec.ts", entries);

        // Config files
        Assert.Contains("apps/claims-web/src/app/core/config/app-config.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/config/app-config.service.ts", entries);
        Assert.Contains("apps/claims-web/src/assets/app-config.example.json", entries);

        // app-config.json must NOT be in the print
        Assert.DoesNotContain("apps/claims-web/src/assets/app-config.json", entries);

        // Guards and interceptors
        Assert.Contains("apps/claims-web/src/app/core/guards/auth.guard.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/interceptors/auth.interceptor.ts", entries);

        // Auth feature
        Assert.Contains("apps/claims-web/src/app/features/auth/auth-callback.component.ts", entries);
        Assert.Contains("apps/claims-web/src/app/features/login/login.component.ts", entries);

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
        Assert.Contains("app-config.json", gitignore);
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
        Assert.Contains("apps/claims-web/src/app/core/auth/auth.models.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/auth.service.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/cognito-client.ts", entries);
        Assert.Contains("apps/claims-web/src/app/core/auth/auth-sync.service.ts", entries);

        // Login component uses AuthService for both web and mobile
        var loginEntry = archive.GetEntry("apps/claims-web/src/app/features/login/login.component.ts");
        Assert.NotNull(loginEntry);
        using var loginStream = loginEntry!.Open();
        using var loginReader = new StreamReader(loginStream, Encoding.UTF8);
        var loginContent = await loginReader.ReadToEndAsync();
        Assert.Contains("AuthService", loginContent);
    }
}
