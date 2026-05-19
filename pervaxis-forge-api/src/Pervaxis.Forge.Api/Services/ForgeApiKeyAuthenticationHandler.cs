using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Pervaxis.Forge.Api.Models.Configuration;

namespace Pervaxis.Forge.Api.Services;

#pragma warning disable CS0618
public sealed class ForgeApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-Api-Key";
    private const string ResolvedKeyCacheKey = "forge:resolved-api-key";
    private static readonly TimeSpan SecretsCacheDuration = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan SecretsCallTimeout = TimeSpan.FromSeconds(10);

    private readonly IAmazonSecretsManager _secretsManager;
    private readonly IOptionsMonitor<ForgeAuthenticationOptions> _authenticationOptions;
    private readonly IOptionsMonitor<ForgeSecretsOptions> _secretsOptions;
    private readonly IMemoryCache _cache;

    public ForgeApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IAmazonSecretsManager secretsManager,
        IOptionsMonitor<ForgeAuthenticationOptions> authenticationOptions,
        IOptionsMonitor<ForgeSecretsOptions> secretsOptions,
        IMemoryCache cache)
        : base(options, logger, encoder, clock)
    {
        _secretsManager = secretsManager;
        _authenticationOptions = authenticationOptions;
        _secretsOptions = secretsOptions;
        _cache = cache;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedKey) ||
            string.IsNullOrWhiteSpace(providedKey))
        {
            return AuthenticateResult.NoResult();
        }

        var expectedKey = await ResolveApiKeyAsync();
        if (string.IsNullOrWhiteSpace(expectedKey) || !string.Equals(providedKey, expectedKey, StringComparison.Ordinal))
        {
            return AuthenticateResult.Fail("Invalid API key.");
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "forge-api-client"),
            new Claim(ClaimTypes.Name, "Forge API Client"),
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    private async Task<string?> ResolveApiKeyAsync()
    {
        var directKey = _authenticationOptions.CurrentValue.ApiKey;
        if (!string.IsNullOrWhiteSpace(directKey))
            return directKey;

        var secrets = _secretsOptions.CurrentValue;
        if (!secrets.UseSecretsManager || string.IsNullOrWhiteSpace(secrets.SecretId))
            return null;

        if (_cache.TryGetValue<string>(ResolvedKeyCacheKey, out var cached))
            return cached;

        try
        {
            using var cts = new CancellationTokenSource(SecretsCallTimeout);
            var response = await _secretsManager.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = secrets.SecretId,
            }, cts.Token);

            if (string.IsNullOrWhiteSpace(response.SecretString))
                return null;

            using var document = JsonDocument.Parse(response.SecretString);
            var resolved = document.RootElement.TryGetProperty(secrets.ApiKeySecretKey, out var value)
                ? value.GetString()
                : null;

            if (!string.IsNullOrWhiteSpace(resolved))
                _cache.Set(ResolvedKeyCacheKey, resolved, SecretsCacheDuration);

            return resolved;
        }
        catch (OperationCanceledException)
        {
            Logger.LogWarning(
                "Secrets Manager call timed out after {TimeoutSeconds}s for secret {SecretId}",
                SecretsCallTimeout.TotalSeconds,
                secrets.SecretId);
            return null;
        }
    }
}
#pragma warning restore CS0618
