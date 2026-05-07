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

namespace Pervaxis.Forge.Api.Data.Entities;

public class Vertical
{
    public Guid Id { get; set; }
    public required string Slug { get; set; }
    public required string DisplayName { get; set; }
    public string? Description { get; set; }
    public required string OwnerTeam { get; set; }
    public required string OwnerEmail { get; set; }
    public required string ComponentPrefix { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public VerticalCloudConfig? CloudConfig { get; set; }
    public VerticalSourceControlConfig? SourceControlConfig { get; set; }
    public VerticalTechDefaults? TechDefaults { get; set; }
    public ICollection<GenerationLog> GenerationLogs { get; set; } = new List<GenerationLog>();
}
