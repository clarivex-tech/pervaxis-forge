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

internal static class VerticalEndpoints
{
    internal static IEndpointRouteBuilder MapVerticalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/verticals")
            .WithTags("Verticals");

        group.MapPost("/", EnrollVertical)
            .WithName("EnrollVertical")
            .WithSummary("Enroll a new vertical")
            .Produces<VerticalResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", ListVerticals)
            .WithName("ListVerticals")
            .WithSummary("List all enrolled verticals")
            .Produces<IEnumerable<VerticalSummaryResponse>>();

        group.MapGet("/{slug}", GetVertical)
            .WithName("GetVertical")
            .WithSummary("Get vertical details by slug")
            .Produces<VerticalResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{slug}", UpdateVertical)
            .WithName("UpdateVertical")
            .WithSummary("Update mutable vertical fields")
            .Produces<VerticalResponse>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{slug}", UnenrollVertical)
            .WithName("UnenrollVertical")
            .WithSummary("Unenroll (soft-delete) a vertical")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{slug}/validate", ValidateConnectivity)
            .WithName("ValidateVerticalConnectivity")
            .WithSummary("Validate cloud and source control connectivity without enrolling")
            .Produces<ConnectivityValidationResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }

    private static IResult EnrollVertical(VerticalEnrollmentRequest request)
        => Results.Problem(statusCode: 501, title: "Not implemented");

    private static IResult ListVerticals()
        => Results.Problem(statusCode: 501, title: "Not implemented");

    private static IResult GetVertical(string slug)
        => Results.Problem(statusCode: 501, title: "Not implemented");

    private static IResult UpdateVertical(string slug, UpdateVerticalRequest request)
        => Results.Problem(statusCode: 501, title: "Not implemented");

    private static IResult UnenrollVertical(string slug)
        => Results.Problem(statusCode: 501, title: "Not implemented");

    private static IResult ValidateConnectivity(string slug, VerticalEnrollmentRequest request)
        => Results.Problem(statusCode: 501, title: "Not implemented");
}
