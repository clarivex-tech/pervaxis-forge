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

public record VerticalTechDefaults
{
    public List<string> Environments { get; init; } = ["test", "accp", "prod"];
    public string DefaultEnvironment { get; init; } = "test";
    public bool GenerateTerraform { get; init; } = true;
    public bool GenerateCdk { get; init; } = true;
    public string? DefaultDbEngine { get; init; }
}
