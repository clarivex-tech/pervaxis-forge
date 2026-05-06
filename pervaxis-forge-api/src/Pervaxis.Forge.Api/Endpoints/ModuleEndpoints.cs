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

namespace Pervaxis.Forge.Api.Endpoints;

internal static class ModuleEndpoints
{
    internal static IEndpointRouteBuilder MapModuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Modules");

        group.MapGet("/modules", ListGenesisModules)
            .WithName("ListGenesisModules")
            .WithSummary("List all available Genesis modules")
            .Produces<IEnumerable<string>>();

        group.MapGet("/canvas-modules", ListCanvasModules)
            .WithName("ListCanvasModules")
            .WithSummary("List all available Canvas modules")
            .Produces<IEnumerable<string>>();

        return app;
    }

    private static IResult ListGenesisModules()
        => Results.Problem(statusCode: 501, title: "Not implemented");

    private static IResult ListCanvasModules()
        => Results.Problem(statusCode: 501, title: "Not implemented");
}
