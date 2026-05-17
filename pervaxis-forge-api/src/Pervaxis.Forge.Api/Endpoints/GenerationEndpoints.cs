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

using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Models.Responses;
using Pervaxis.Forge.Api.Services;

namespace Pervaxis.Forge.Api.Endpoints;

internal static class GenerationEndpoints
{
    internal static IEndpointRouteBuilder MapGenerationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/generate")
            .WithTags("Generation");

        group.MapPost("/", GenerateSingle)
            .WithName("GenerateService")
            .WithSummary("Generate a single service within an enrolled vertical")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/batch", GenerateBatch)
            .WithName("GenerateServiceBatch")
            .WithSummary("Generate multiple services within an enrolled vertical")
            .Produces(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/validate", ValidateManifest)
            .WithName("ValidateManifest")
            .WithSummary("Validate a generation request and preview derived names")
            .Produces<ValidationPreviewResult>()
            .Produces<ValidationPreviewResult>(StatusCodes.Status422UnprocessableEntity)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/audit/{slug}", GetAuditLog)
            .WithName("GetGenerationAuditLog")
            .WithSummary("Get generation history for an enrolled vertical")
            .Produces<IReadOnlyList<GenerationAuditEntry>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        var servicesGroup = app.MapGroup("/api/v1/verticals/{slug}/services")
            .WithTags("Generation");

        servicesGroup.MapGet("/", ListGeneratedServices)
            .WithName("ListGeneratedServices")
            .WithSummary("List generated services for a vertical")
            .Produces<IReadOnlyList<GeneratedServiceResponse>>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        servicesGroup.MapPost("/{id:guid}/regenerate", RegenerateService)
            .WithName("RegenerateGeneratedService")
            .WithSummary("Regenerate a service ZIP from the stored manifest")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static async Task<IResult> GenerateSingle(GenerationRequest request, IGenerationService generationService, HttpContext httpContext, CancellationToken ct = default)
    {
        try
        {
            var generatedBy = ResolveGeneratedBy(httpContext);
            var (zip, result) = await generationService.GenerateAsync(request, generatedBy, ct);
            httpContext.Response.Headers["X-Generation-Service-Name"] = result.ServiceName;
            httpContext.Response.Headers["X-Generation-Vertical"] = result.VerticalSlug;
            httpContext.Response.Headers["X-Generation-Timestamp"] = result.GeneratedAt.ToString("O");
            if (result.GitHubRepoUrl is not null)
                httpContext.Response.Headers["X-Generation-GitHub-Url"] = result.GitHubRepoUrl;
            return Results.File(zip, "application/zip", $"{request.Name}-scaffold.zip");
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { errors = new[] { ex.Message } });
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                return Results.Conflict(new { errors = new[] { ex.Message } });
            return Results.UnprocessableEntity(new { errors = new[] { ex.Message } });
        }
    }

    private static async Task<IResult> GenerateBatch(BatchGenerationRequest request, IGenerationService generationService, HttpContext httpContext, CancellationToken ct = default)
    {
        try
        {
            var (zip, result) = await generationService.GenerateBatchAsync(request, ct);
            httpContext.Response.Headers["X-Generation-Total"] = result.TotalServices.ToString();
            httpContext.Response.Headers["X-Generation-Succeeded"] = result.SucceededCount.ToString();
            httpContext.Response.Headers["X-Generation-Failed"] = result.FailedCount.ToString();
            return Results.File(zip, "application/zip", $"{request.VerticalSlug}-batch-scaffold.zip");
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { errors = new[] { ex.Message } });
        }
        catch (InvalidOperationException ex)
        {
            return Results.UnprocessableEntity(new { errors = new[] { ex.Message } });
        }
    }

    private static async Task<IResult> ValidateManifest(GenerationRequest request, IGenerationService generationService, CancellationToken ct = default)
    {
        try
        {
            var preview = await generationService.ValidateAsync(request, ct);
            return preview.IsValid
                ? Results.Ok(preview)
                : Results.UnprocessableEntity(preview);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { errors = new[] { ex.Message } });
        }
    }

    private static async Task<IResult> GetAuditLog(string slug, IGenerationService generationService, CancellationToken ct = default)
    {
        var entries = await generationService.GetAuditLogAsync(slug, ct);
        return Results.Ok(entries);
    }

    private static async Task<IResult> ListGeneratedServices(string slug, IGenerationService generationService, CancellationToken ct = default)
    {
        try
        {
            var services = await generationService.ListGeneratedServicesAsync(slug, ct);
            return Results.Ok(services);
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { errors = new[] { ex.Message } });
        }
    }

    private static async Task<IResult> RegenerateService(string slug, Guid id, IGenerationService generationService, CancellationToken ct = default)
    {
        try
        {
            var (zip, service) = await generationService.RegenerateAsync(slug, id, ct);
            return Results.File(zip, "application/zip", $"{service.ServiceName}-regenerated.zip");
        }
        catch (KeyNotFoundException ex)
        {
            return Results.NotFound(new { errors = new[] { ex.Message } });
        }
        catch (InvalidOperationException ex)
        {
            return Results.UnprocessableEntity(new { errors = new[] { ex.Message } });
        }
    }

    private static string ResolveGeneratedBy(HttpContext httpContext)
    {
        var userName = httpContext.User?.Identity?.Name;
        if (!string.IsNullOrWhiteSpace(userName))
            return userName;

        if (httpContext.Request.Headers.TryGetValue("X-Api-Key-Id", out var apiKeyId) && !string.IsNullOrWhiteSpace(apiKeyId))
            return apiKeyId.ToString();

        if (httpContext.Request.Headers.TryGetValue("X-Forwarded-User", out var forwardedUser) && !string.IsNullOrWhiteSpace(forwardedUser))
            return forwardedUser.ToString();

        if (httpContext.Request.Headers.TryGetValue("X-Amzn-Oidc-Identity", out var oidcIdentity) && !string.IsNullOrWhiteSpace(oidcIdentity))
            return oidcIdentity.ToString();

        return "forge-api";
    }
}
