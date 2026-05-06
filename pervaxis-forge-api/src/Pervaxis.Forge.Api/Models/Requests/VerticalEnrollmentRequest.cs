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

namespace Pervaxis.Forge.Api.Models.Requests;

public record VerticalEnrollmentRequest
{
    public required string Slug { get; init; }
    public required string DisplayName { get; init; }
    public required string Description { get; init; }
    public required string OwnerTeam { get; init; }
    public required string OwnerEmail { get; init; }
    public required CloudProviderConfig CloudProvider { get; init; }
    public required SourceControlConfig SourceControl { get; init; }
    public required VerticalTechDefaults TechDefaults { get; init; }
}
