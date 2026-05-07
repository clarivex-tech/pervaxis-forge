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

    private static async Task<IResult> EnrollVertical(
        VerticalEnrollmentRequest request,
        IVerticalService service,
        CancellationToken ct)
    {
        try
        {
            var vertical = await service.EnrollAsync(request, ct);
            return Results.Created($"/api/v1/verticals/{vertical.Slug}", vertical);
        }
        catch (ValidationException ex)
        {
            var errors = ex.Failures
                .GroupBy(f => f.Field)
                .ToDictionary(g => g.Key, g => g.Select(f => f.Message).ToArray());

            return Results.ValidationProblem(errors);
        }
        catch (SlugConflictException ex)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Slug already exists",
                detail: ex.Message);
        }
    }

    private static async Task<IResult> ListVerticals(
        IVerticalService service,
        CancellationToken ct)
    {
        var verticals = await service.ListAsync(ct);
        return Results.Ok(verticals);
    }

    private static async Task<IResult> GetVertical(
        string slug,
        IVerticalService service,
        CancellationToken ct)
    {
        var vertical = await service.GetAsync(slug, ct);
        return vertical is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vertical not found",
                detail: $"No vertical with slug '{slug}'.")
            : Results.Ok(vertical);
    }

    private static async Task<IResult> UpdateVertical(
        string slug,
        UpdateVerticalRequest request,
        IVerticalService service,
        CancellationToken ct)
    {
        var vertical = await service.UpdateAsync(slug, request, ct);
        return vertical is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vertical not found",
                detail: $"No vertical with slug '{slug}'.")
            : Results.Ok(vertical);
    }

    private static async Task<IResult> UnenrollVertical(
        string slug,
        IVerticalService service,
        CancellationToken ct)
    {
        var unenrolled = await service.UnenrollAsync(slug, ct);
        return unenrolled
            ? Results.NoContent()
            : Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Vertical not found",
                detail: $"No vertical with slug '{slug}'.");
    }

    // TODO: implement once IVerticalConnectivityValidator is wired (next session).
    private static IResult ValidateConnectivity(string slug, VerticalEnrollmentRequest request)
        => Results.Problem(statusCode: StatusCodes.Status501NotImplemented, title: "Not implemented");
}
