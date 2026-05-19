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
using Amazon.SecretsManager;
using Amazon.SecurityToken;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.OpenApi.Models;
using Octokit;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Endpoints;
using Pervaxis.Forge.Api.Models.Configuration;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Services;
using Pervaxis.Forge.Engine.Generation;
using Amazon.Lambda.AspNetCoreServer;
using System.Text.Json;
using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddDbContextPool<ForgeDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("ForgeDb"),
        npgsql => npgsql.EnableRetryOnFailure(3)));

builder.Services.AddScoped<IVerticalService, VerticalService>();

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
builder.Services.AddOutputCache();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "ForgeApiKey";
    options.DefaultChallengeScheme = "ForgeApiKey";
})
    .AddScheme<AuthenticationSchemeOptions, ForgeApiKeyAuthenticationHandler>("ForgeApiKey", _ => { });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes("ForgeApiKey")
        .RequireAuthenticatedUser()
        .Build();
});
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

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled exception while processing {Method} {Path}", context.Request.Method, context.Request.Path);

        if (context.Response.HasStarted)
        {
            throw;
        }

        context.Response.Clear();
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";

        var payload = JsonSerializer.Serialize(new
        {
            type = "about:blank",
            title = "Internal Server Error",
            status = StatusCodes.Status500InternalServerError,
            detail = "The request pipeline failed unexpectedly.",
        });

        await context.Response.WriteAsync(payload);
    }
});

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Forge:EnableSwagger"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Pervaxis Forge API v1"));
}

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseOutputCache();
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
app.Use(async (context, next) =>
{
    context.Response.OnStarting(() =>
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";

        if (!app.Environment.IsDevelopment())
        {
            headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";
        }

        return Task.CompletedTask;
    });

    await next();
});
app.UseCors(ForgeUiCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();

await ApplyPendingMigrationsAsync(app.Services);

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
