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

using Amazon.Extensions.NETCore.Setup;
using Amazon.AspNetCore.DataProtection.SSM;
using Amazon.SecretsManager;
using Amazon.SecurityToken;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;
using System.Threading.RateLimiting;
using Octokit;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Endpoints;
using Pervaxis.Forge.Api.Infrastructure.Extensions;
using Pervaxis.Forge.Api.Infrastructure.Http;
using Pervaxis.Forge.Api.Infrastructure.Middleware;
using Pervaxis.Forge.Api.Infrastructure.Security;
using Pervaxis.Forge.Api.Models.Configuration;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Services;
using Pervaxis.Forge.Engine.Generation;
using Amazon.Lambda.AspNetCoreServer;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

var isRunningInLambda = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("AWS_LAMBDA_FUNCTION_NAME"));
var isLocalMode = builder.Configuration.GetValue<bool>("Forge:LocalMode");
var dataProtectionEnabled = !isRunningInLambda && !isLocalMode && builder.Configuration.GetValue<bool>("Forge:DataProtection:Enabled");
var dataProtectionPrefix = builder.Configuration["Forge:DataProtection:Prefix"] ?? "/Pervaxis/Forge/DataProtection";
var dataProtectionKmsKeyId = builder.Configuration["Forge:DataProtection:KmsKeyId"];

if (dataProtectionEnabled)
{
    builder.Services.AddDataProtection()
        .SetApplicationName("Pervaxis.Forge.Api")
        .PersistKeysToAWSSystemsManager(dataProtectionPrefix, options =>
        {
            if (!string.IsNullOrWhiteSpace(dataProtectionKmsKeyId))
            {
                options.KMSKeyId = dataProtectionKmsKeyId;
            }
        });
}

builder.AddForgeSerilog();
builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection", LogLevel.Information);
builder.Logging.AddFilter("Amazon.AspNetCore.DataProtection.SSM", LogLevel.Information);
builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection.Repositories.EphemeralXmlRepository", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.AspNetCore.DataProtection.KeyManagement.XmlKeyManager", LogLevel.Warning);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

if (isLocalMode)
{
    builder.Services.AddDbContext<ForgeDbContext>(options =>
        options.UseInMemoryDatabase("forge-local"));
}
else
{
    builder.Services.AddDbContextPool<ForgeDbContext>(options =>
        options.UseNpgsql(
            builder.Configuration.GetConnectionString("ForgeDb"),
            npgsql =>
            {
                npgsql.EnableRetryOnFailure(3);
                npgsql.CommandTimeout(10);
            }));
}

builder.Services.AddScoped<IVerticalService, VerticalService>();

builder.Services.AddHttpClient<Pervaxis.Forge.Engine.NuGet.NuGetVersionResolver>(client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "Pervaxis-Forge");
    client.Timeout = TimeSpan.FromSeconds(10);
});
builder.Services.AddSingleton<Pervaxis.Forge.Engine.NuGet.INuGetVersionResolver, Pervaxis.Forge.Engine.NuGet.NuGetVersionResolver>();

builder.Services.AddScoped<PrintGenerator>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IGenerationService, GenerationService>();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSecurityTokenService>();
builder.Services.AddAWSService<IAmazonSecretsManager>();
builder.Services.AddOptions<ForgeAuthenticationOptions>()
    .BindConfiguration(ForgeAuthenticationOptions.SectionName);
builder.Services.AddOptions<ForgeSecretsOptions>()
    .BindConfiguration(ForgeSecretsOptions.SectionName);
builder.Services.AddOptions<ForgeDataClassificationOptions>()
    .BindConfiguration(ForgeDataClassificationOptions.SectionName);
builder.Services.AddSingleton<ForgeDataRedaction>();
builder.Services.AddOptions<ForgeOutputCachingOptions>()
    .BindConfiguration(ForgeOutputCachingOptions.SectionName);
builder.Services.AddMemoryCache();
builder.Services.AddOutputCache();
builder.Services.AddOptions<ForgeRateLimitingOptions>()
    .BindConfiguration(ForgeRateLimitingOptions.SectionName);
builder.Services.AddRateLimiter(limiterOptions =>
{
    var rateLimiting = builder.Configuration.GetSection(ForgeRateLimitingOptions.SectionName)
        .Get<ForgeRateLimitingOptions>() ?? new ForgeRateLimitingOptions();

    if (rateLimiting.Enabled)
    {
        limiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        limiterOptions.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                factory: _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = Math.Max(1, rateLimiting.PermitLimit),
                    Window = TimeSpan.FromMinutes(Math.Max(1, rateLimiting.WindowMinutes)),
                    QueueLimit = 0,
                    AutoReplenishment = true,
                }));
    }
});
builder.Services.AddForgeAuthentication(builder.Configuration);
builder.Services.Configure<ForgeOidcOptions>(builder.Configuration.GetSection(ForgeOidcOptions.SectionName));
builder.Services.Configure<ForgeTenantOptions>(builder.Configuration.GetSection(ForgeTenantOptions.SectionName));
builder.Services.Configure<ForgeOutboxOptions>(builder.Configuration.GetSection(ForgeOutboxOptions.SectionName));
builder.Services.AddSingleton<IForgeHashingService, ForgeHashingService>();
builder.Services.AddSingleton<Func<string, IGitHubClient>>(
    _ => token => new GitHubClient(new ProductHeaderValue("pervaxis-forge"))
    {
        Credentials = new Credentials(token)
    });
builder.Services.AddScoped<IVerticalConnectivityValidator, VerticalConnectivityValidator>();

const string ForgeUiCorsPolicy = "ForgeUi";
builder.Services.AddCors(options =>
{
    var allowedOrigins = builder.Configuration
        .GetSection("Forge:Cors:AllowedOrigins")
        .Get<string[]>() ?? ["http://localhost:4200"];

    options.AddPolicy(ForgeUiCorsPolicy, policy => policy
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Pervaxis Forge API",
        Version = "v1",
        Description = "Internal admin API for vertical enrollment and service generation. " +
                      "Forge is the provisioning backbone for all Clarivex business verticals.",
        Contact = new OpenApiContact
        {
            Name = "Clarivex Technologies",
            Url = new Uri("https://clarivex.tech")
        }
    });
});

// Cross-cutting concerns: resilience options, tracing, validation, versioning
builder.Services.Configure<ForgeResilienceOptions>(
    builder.Configuration.GetSection(ForgeResilienceOptions.SectionName));
builder.Services.AddForgeTracing(builder.Configuration);
builder.Services.AddForgeMetrics(builder.Configuration);
builder.Services.AddForgeValidation();
builder.Services.AddForgeVersioning();

// HTTP infrastructure: context accessor, delegating handlers, typed clients
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<CorrelationIdHandler>();
builder.Services.AddTransient<JwtPropagationHandler>();
builder.Services.AddTransient<ExternalAuthHandler>();
builder.Services.AddTransient<HttpLoggingHandler>();

builder.Services.AddHttpClient<IGitHubHttpClient, GitHubHttpClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GitHub:BaseUrl"] ?? "https://api.github.com");
    client.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
})
.AddHttpMessageHandler<CorrelationIdHandler>()
.AddHttpMessageHandler<HttpLoggingHandler>()
.AddForgeResilience(builder.Configuration.GetSection("Resilience:GitHub"));

builder.Services.AddHttpClient<IForgeInternalHttpClient, ForgeInternalHttpClient>(client =>
{
    // Base address configured per deployment
})
.AddHttpMessageHandler<CorrelationIdHandler>()
.AddHttpMessageHandler<JwtPropagationHandler>()
.AddHttpMessageHandler<HttpLoggingHandler>()
.AddForgeResilience(builder.Configuration.GetSection("Resilience:Internal"));

builder.Services.AddHttpClient<IForgeExternalHttpClient, ForgeExternalHttpClient>(client =>
{
    var baseUrl = builder.Configuration["ExternalServices:BaseUrl"];
    if (!string.IsNullOrEmpty(baseUrl))
        client.BaseAddress = new Uri(baseUrl);
})
.AddHttpMessageHandler<CorrelationIdHandler>()
.AddHttpMessageHandler<ExternalAuthHandler>()
.AddHttpMessageHandler<HttpLoggingHandler>()
.AddForgeResilience(builder.Configuration.GetSection("Resilience:External"));

var app = builder.Build();

app.Logger.LogInformation(
    dataProtectionEnabled
        ? "Data Protection configured for prefix {Prefix} with KMS key {KmsKeyId}"
        : "Data Protection SSM persistence is disabled in this environment",
    dataProtectionPrefix,
    string.IsNullOrWhiteSpace(dataProtectionKmsKeyId) ? "<default AWS-managed SSM encryption>" : dataProtectionKmsKeyId);

// Cross-cutting middleware (order is critical)
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseMiddleware<JwtPropagationMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Forge:EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pervaxis Forge API v1"));
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseOutputCache();
app.UseRateLimiter();
app.Use(async (context, next) =>
{
    var startedAt = Stopwatch.GetTimestamp();
    await next();

    var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
    var actor = context.User.Identity?.IsAuthenticated == true
        ? context.User.Identity?.Name ?? "authenticated-user"
        : "anonymous";

    app.Logger.LogInformation(
        "Audit event {AuditAction} {Method} {Path} {StatusCode} {ElapsedMs}ms {Actor} {TraceId}",
        "request",
        context.Request.Method,
        context.Request.Path.Value,
        context.Response.StatusCode,
        elapsedMs,
        actor,
        context.TraceIdentifier);
});
app.UseCors(ForgeUiCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

// Keep Lambda startup lean: schema migrations must run out of band because
// they can exceed the cold-start budget and cause INIT timeouts.
if (!isLocalMode && !isRunningInLambda && app.Environment.IsDevelopment())
{
    await ApplyPendingMigrationsAsync(app.Services);
}

app.MapVerticalEndpoints();
app.MapGenerationEndpoints();
app.MapModuleEndpoints();

if (app.Environment.IsDevelopment())
{
    await SeedSampleVerticalsAsync(app.Services);
}

app.Run();

static async Task SeedSampleVerticalsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var verticalService = scope.ServiceProvider.GetRequiredService<IVerticalService>();

    if (await verticalService.ListAsync() is { Count: > 0 })
    {
        return;
    }

    var sampleVerticals = new[]
    {
        new VerticalEnrollmentRequest
        {
            Slug = "clarivex-ops",
            DisplayName = "Clarivex Operations",
            Description = "Operations and internal platform vertical.",
            OwnerTeam = "Platform Ops",
            OwnerEmail = "ops@clarivex.tech",
            ComponentPrefix = "CLV",
            CloudProvider = new CloudProviderConfig
            {
                Provider = "AWS",
                AwsAccountId = "111111111111",
                IamRoleArn = "arn:aws:iam::111111111111:role/forge-dev-ops",
                DefaultRegion = "us-east-1",
            },
            SourceControl = new SourceControlConfig
            {
                Platform = "GitHub",
                GitHubOrg = "clarivex-tech",
                AccessToken = "ghp_sampletoken_ops",
                DefaultVisibility = "Private",
                DefaultBranchProtection = true,
            },
            TechDefaults = new VerticalTechDefaults
            {
                Environments = ["dev", "test", "prod"],
                DefaultEnvironment = "dev",
                GenerateTerraform = true,
                GenerateCdk = true,
                DefaultDbEngine = "postgresql",
            },
        },
        new VerticalEnrollmentRequest
        {
            Slug = "clarivex-analytics",
            DisplayName = "Clarivex Analytics",
            Description = "Analytics and reporting vertical.",
            OwnerTeam = "Data Platform",
            OwnerEmail = "data@clarivex.tech",
            ComponentPrefix = "CNA",
            CloudProvider = new CloudProviderConfig
            {
                Provider = "AWS",
                AwsAccountId = "222222222222",
                IamRoleArn = "arn:aws:iam::222222222222:role/forge-dev-analytics",
                DefaultRegion = "us-east-1",
            },
            SourceControl = new SourceControlConfig
            {
                Platform = "GitHub",
                GitHubOrg = "clarivex-tech",
                AccessToken = "ghp_sampletoken_analytics",
                DefaultVisibility = "Private",
                DefaultBranchProtection = true,
            },
            TechDefaults = new VerticalTechDefaults
            {
                Environments = ["dev", "stage", "prod"],
                DefaultEnvironment = "dev",
                GenerateTerraform = true,
                GenerateCdk = true,
                DefaultDbEngine = "postgresql",
            },
        },
        new VerticalEnrollmentRequest
        {
            Slug = "clarivex-customer-portal",
            DisplayName = "Clarivex Customer Portal",
            Description = "Customer-facing portal vertical.",
            OwnerTeam = "Customer Experience",
            OwnerEmail = "cx@clarivex.tech",
            ComponentPrefix = "CCP",
            CloudProvider = new CloudProviderConfig
            {
                Provider = "AWS",
                AwsAccountId = "333333333333",
                IamRoleArn = "arn:aws:iam::333333333333:role/forge-dev-customer-portal",
                DefaultRegion = "us-east-1",
            },
            SourceControl = new SourceControlConfig
            {
                Platform = "GitHub",
                GitHubOrg = "clarivex-tech",
                AccessToken = "ghp_sampletoken_portal",
                DefaultVisibility = "Private",
                DefaultBranchProtection = true,
            },
            TechDefaults = new VerticalTechDefaults
            {
                Environments = ["dev", "qa", "prod"],
                DefaultEnvironment = "dev",
                GenerateTerraform = true,
                GenerateCdk = true,
                DefaultDbEngine = "postgresql",
            },
        },
    };

    foreach (var request in sampleVerticals)
    {
        await verticalService.EnrollAsync(request);
    }
}

static async Task ApplyPendingMigrationsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ForgeDbContext>();
    await db.Database.MigrateAsync();
}
