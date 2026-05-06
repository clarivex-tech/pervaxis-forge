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

public class VerticalSourceControlConfig
{
    public Guid Id { get; set; }
    public Guid VerticalId { get; set; }
    public required string Platform { get; set; }
    public string? GitHubOrg { get; set; }
    public string? AccessToken { get; set; }  // stored encrypted via Data Protection
    public required string DefaultVisibility { get; set; }
    public bool DefaultBranchProtection { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Vertical Vertical { get; set; } = null!;
}
