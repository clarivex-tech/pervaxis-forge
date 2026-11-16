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

namespace Pervaxis.Forge.Engine.Manifest;

/// <summary>Public browser authentication settings for an Angular monolith. Never contains credentials.</summary>
public sealed record AngularAuthConfig
{
    /// <summary>Required provider: Cognito.</summary>
    public required string Provider { get; init; }

    /// <summary>Cognito pool ID. Its prefix supplies the AWS region.</summary>
    public string? UserPoolId { get; init; }

    /// <summary>Public app client ID (without a client secret).</summary>
    public string? ClientId { get; init; }

    /// <summary>HTTPS origin of the Cognito managed login domain.</summary>
    public string? Domain { get; init; }

    /// <summary>Allowed OAuth scopes, including openid.</summary>
    public IReadOnlyList<string> Scopes { get; init; } = [];

    /// <summary>Registered absolute callback URL ending in /auth/callback.</summary>
    public string? RedirectUri { get; init; }

    /// <summary>Registered absolute sign-out return URL on the same app origin.</summary>
    public string? PostLogoutRedirectUri { get; init; }
}
