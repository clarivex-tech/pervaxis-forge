// Copyright (C) 2026 Clarivex Technologies Private Limited. All Rights Reserved.

namespace Pervaxis.Forge.Engine.Manifest;

/// <summary>Public deployment inputs for the Cognito pre-token-generation Lambda.</summary>
public sealed record TokenEnrichmentConfig
{
    /// <summary>Region containing the existing user pool and generated Lambda.</summary>
    public required string Region { get; init; }

    /// <summary>Existing Cognito user pool ID. Contains no credentials.</summary>
    public required string UserPoolId { get; init; }
}
