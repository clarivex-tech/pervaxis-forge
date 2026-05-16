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

public class GeneratedService
{
    public Guid Id { get; set; }
    public Guid VerticalId { get; set; }
    public required string ServiceName { get; set; }
    public required string ServiceType { get; set; }
    public required JsonDocument ManifestJson { get; set; }
    public required string CloudProvider { get; set; }
    public DateTimeOffset GeneratedAt { get; set; }
    public required string GeneratedBy { get; set; }

    public Vertical Vertical { get; set; } = null!;
}
