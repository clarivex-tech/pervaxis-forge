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

public class DeploymentOutput
{
    public Guid Id { get; set; }
    public Guid GenerationLogId { get; set; }
    public required string ServiceName { get; set; }
    public required string ResourceType { get; set; }
    public required string ResourceName { get; set; }
    public string? EndpointOrArn { get; set; }
    public DateTime CreatedAt { get; set; }

    public GenerationLog GenerationLog { get; set; } = null!;
}
