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

using OpenTelemetry.Metrics;
using Pervaxis.Forge.Api.Models.Configuration;

namespace Pervaxis.Forge.Api.Infrastructure.Extensions;

public static class MetricsExtensions
{
    public static IServiceCollection AddForgeMetrics(
        this IServiceCollection services, IConfiguration configuration)
    {
        var options = new ForgeTracingOptions();
        configuration.GetSection(ForgeTracingOptions.SectionName).Bind(options);

        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    .AddMeter("Pervaxis.*")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddOtlpExporter(otlp => otlp.Endpoint = new Uri(options.OtlpEndpoint));

                if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
                {
                    metrics.AddConsoleExporter();
                }
            });

        return services;
    }
}
