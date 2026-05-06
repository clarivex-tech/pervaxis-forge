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
            .Produces<GenerationResult>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/batch", GenerateBatch)
            .WithName("GenerateServiceBatch")
            .WithSummary("Generate multiple services within an enrolled vertical")
            .Produces<BatchGenerationResult>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static IResult GenerateSingle(GenerationRequest request)
        => Results.Problem(statusCode: 501, title: "Not implemented");

    private static IResult GenerateBatch(BatchGenerationRequest request)
        => Results.Problem(statusCode: 501, title: "Not implemented");
}
