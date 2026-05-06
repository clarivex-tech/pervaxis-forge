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

public class VerticalTechDefaults
{
    public Guid Id { get; set; }
    public Guid VerticalId { get; set; }
    public string[] Environments { get; set; } = ["test", "accp", "prod"];
    public required string DefaultEnvironment { get; set; }
    public bool GenerateTerraform { get; set; } = true;
    public bool GenerateCdk { get; set; } = true;
    public string? DefaultDbEngine { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Vertical Vertical { get; set; } = null!;
}
