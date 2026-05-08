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
using Amazon.SecurityToken;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Octokit;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Endpoints;
using Pervaxis.Forge.Api.Services;
using Amazon.Lambda.AspNetCoreServer;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

builder.Services.AddDbContext<ForgeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ForgeDb")));

builder.Services.AddScoped<IVerticalService, VerticalService>();

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonSecurityTokenService>();
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
        .GetSection("Forge:AllowedOrigins")
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
app.UseCors(ForgeUiCorsPolicy);

app.MapVerticalEndpoints();
app.MapGenerationEndpoints();
app.MapModuleEndpoints();

app.Run();
