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
using Pervaxis.Forge.Engine.Manifest;
namespace Pervaxis.Forge.Engine.Tests;
internal static class TestTenancy
{
    internal static readonly Guid InitialTenantId = Guid.Parse("3f2b8c1e-6a4d-4e2f-9b1a-7c5d8e9f0a1b");
    internal static CognitoConfig Cognito => new() { UserPoolId = "us-east-1_Example", ClientIds = ["exampleclient123"] };
    internal static AngularAuthConfig AngularAuth => new()
    {
        Provider = "Cognito", UserPoolId = "us-east-1_Example", ClientId = "exampleclient123",
        Domain = "https://example.auth.us-east-1.amazoncognito.com", Scopes = ["openid", "email", "phone"],
        RedirectUri = "http://localhost:4200/auth/callback", PostLogoutRedirectUri = "http://localhost:4200/login"
    };
}
