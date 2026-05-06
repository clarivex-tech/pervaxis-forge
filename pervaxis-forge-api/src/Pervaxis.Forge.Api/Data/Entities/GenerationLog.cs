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

using System.Text.Json;

namespace Pervaxis.Forge.Api.Data.Entities;

public class GenerationLog
{
    public Guid Id { get; set; }
    public Guid VerticalId { get; set; }
    public required JsonDocument Manifest { get; set; }
    public int ServiceCount { get; set; }
    public bool InfrastructureDeployed { get; set; }
    public bool GitHubReposCreated { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }

    public Vertical Vertical { get; set; } = null!;
    public ICollection<DeploymentOutput> DeploymentOutputs { get; set; } = new List<DeploymentOutput>();
}
