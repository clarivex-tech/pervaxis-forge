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

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pervaxis.Forge.Api.Models.Configuration;
using Pervaxis.Forge.Api.Services;

namespace Pervaxis.Forge.Api.Infrastructure.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddForgeAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var auth0Options = new ForgeAuth0Options();
        configuration.GetSection(ForgeAuth0Options.SectionName).Bind(auth0Options);

        var authOptions = new ForgeAuthenticationOptions();
        configuration.GetSection(ForgeAuthenticationOptions.SectionName).Bind(authOptions);

        var scheme = authOptions.Scheme;

        // Validate Auth0 config only when JWT is needed
        if (scheme is "Bearer" or "Both")
        {
            if (string.IsNullOrEmpty(auth0Options.Authority))
                throw new InvalidOperationException(
                    $"Configuration key '{ForgeAuth0Options.SectionName}:Authority' is required when Scheme is '{scheme}'.");

            if (string.IsNullOrEmpty(auth0Options.Audience))
                throw new InvalidOperationException(
                    $"Configuration key '{ForgeAuth0Options.SectionName}:Audience' is required when Scheme is '{scheme}'.");
        }

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = scheme switch
            {
                "Bearer" => JwtBearerDefaults.AuthenticationScheme,
                "Both" => "ForgeMultiScheme",
                _ => "ForgeApiKey"
            };
            options.DefaultChallengeScheme = options.DefaultAuthenticateScheme;
        });

        // Always register ForgeApiKey scheme
        authBuilder.AddScheme<AuthenticationSchemeOptions, ForgeApiKeyAuthenticationHandler>(
            "ForgeApiKey", _ => { });

        // Register JwtBearer when needed
        if (scheme is "Bearer" or "Both")
        {
            authBuilder.AddJwtBearer(options =>
            {
                options.Authority = auth0Options.Authority;
                options.Audience = auth0Options.Audience;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                };
            });
        }

        // Register PolicyScheme for "Both" mode
        if (scheme is "Both")
        {
            authBuilder.AddPolicyScheme("ForgeMultiScheme", "ForgeMultiScheme", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
                    if (!string.IsNullOrEmpty(authHeader) &&
                        authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    {
                        return JwtBearerDefaults.AuthenticationScheme;
                    }
                    return "ForgeApiKey";
                };
            });
        }

        // Authorization policy
        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .AddAuthenticationSchemes(scheme switch
                {
                    "Bearer" => JwtBearerDefaults.AuthenticationScheme,
                    "Both" => "ForgeMultiScheme",
                    _ => "ForgeApiKey"
                })
                .RequireAuthenticatedUser()
                .Build();
        });

        // Add Swagger Bearer security definition when JWT is active
        if (scheme is "Bearer" or "Both")
        {
            services.ConfigureSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
        }

        return services;
    }
}
