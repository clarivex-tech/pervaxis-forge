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

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Endpoints;
using Pervaxis.Forge.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Database ─────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ForgeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("ForgeDb")));

// ── Data Protection ──────────────────────────────────────────────────────────
// Dev: keys on local disk. Prod key store (S3/Secrets Manager) is a Phase 3 task.
var keysPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "Pervaxis.Forge", "keys");

builder.Services.AddDataProtection()
    .SetApplicationName("Pervaxis.Forge.Api")
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath));

// ── Domain services ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IVerticalService, VerticalService>();

// ── CORS ─────────────────────────────────────────────────────────────────────
// Allows the Launchpad Angular app (default http://localhost:4200) to call the BFF
// once it swaps its mock VerticalApiService for the real one. Origins come from
// Forge:AllowedOrigins so deployed environments can override without code changes.
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

// ── OpenAPI / Swagger ─────────────────────────────────────────────────────────
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

// ── App pipeline ──────────────────────────────────────────────────────────────
var app = builder.Build();

// Swagger UI: always in dev; opt-in via Forge:EnableSwagger in other environments.
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
