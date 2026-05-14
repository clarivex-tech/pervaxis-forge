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

        return app;
    }

    private static async Task<IResult> GenerateSingle(GenerationRequest request, IGenerationService generationService, HttpContext httpContext, CancellationToken ct = default)
    {
        try
        {
            var (zip, result) = await generationService.GenerateAsync(request, ct);
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
}
