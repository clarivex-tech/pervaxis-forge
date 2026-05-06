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

namespace Pervaxis.Forge.Api.Models.Responses;

public record VerticalSummaryResponse
{
    public required Guid Id { get; init; }
    public required string Slug { get; init; }
    public required string DisplayName { get; init; }
    public string? Description { get; init; }
    public required string CloudProvider { get; init; }
    public required string SourceControl { get; init; }
    public int ServiceCount { get; init; }
    public required DateTimeOffset EnrolledAt { get; init; }
}
