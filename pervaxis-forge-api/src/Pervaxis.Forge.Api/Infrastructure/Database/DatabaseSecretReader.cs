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

using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Npgsql;
using System.Text.Json;

namespace Pervaxis.Forge.Api.Infrastructure.Database;

/// <summary>
/// Reads and parses the Forge database connection details from AWS Secrets Manager.
///
/// Expected secret format (JSON):
/// {
///   "ConnectionStrings__ForgeDb": "Host=...;Database=forge_dev;Username=postgres;Password=...;Port=5432;..."
/// }
///
/// Optionally, a separate credentials secret can override only username + password:
/// {
///   "username": "postgres",
///   "password": "rotated-password"
/// }
/// </summary>
public sealed class DatabaseSecretReader(
    IAmazonSecretsManager client,
    string secretId,
    string? rootCertificate = null,
    string? credentialsSecretId = null)
{
    public async Task<NpgsqlConnectionStringBuilder> ReadAsync(CancellationToken cancellationToken)
    {
        var response = await client.GetSecretValueAsync(new GetSecretValueRequest
        {
            SecretId = secretId,
            VersionStage = "AWSCURRENT"
        }, cancellationToken).ConfigureAwait(false);

        var connection = Parse(response.SecretString);

        if (!string.IsNullOrWhiteSpace(credentialsSecretId))
        {
            var credentials = await client.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = credentialsSecretId,
                VersionStage = "AWSCURRENT"
            }, cancellationToken).ConfigureAwait(false);
            ApplyCredentials(connection, credentials.SecretString);
        }

        if (!string.IsNullOrWhiteSpace(rootCertificate))
            connection.RootCertificate = rootCertificate;

        return connection;
    }

    public static NpgsqlConnectionStringBuilder Parse(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("The database secret must contain a JSON string value.");

        try
        {
            using var document = JsonDocument.Parse(secret);
            if (document.RootElement.ValueKind != JsonValueKind.Object
                || !document.RootElement.TryGetProperty("ConnectionStrings__ForgeDb", out var value)
                || value.ValueKind != JsonValueKind.String)
                throw new InvalidOperationException("The database secret requires ConnectionStrings__ForgeDb.");

            var connection = new NpgsqlConnectionStringBuilder(value.GetString());
            if (string.IsNullOrWhiteSpace(connection.Host)
                || string.IsNullOrWhiteSpace(connection.Database)
                || string.IsNullOrWhiteSpace(connection.Username)
                || string.IsNullOrWhiteSpace(connection.Password))
                throw new InvalidOperationException("The database secret requires host, database, username and password.");

            return connection;
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            throw new InvalidOperationException("The database secret has an invalid format.");
        }
    }

    public static void ApplyCredentials(NpgsqlConnectionStringBuilder connection, string? secret)
    {
        try
        {
            using var document = JsonDocument.Parse(secret ?? "{}");
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object
                || !root.TryGetProperty("username", out var username)
                || !root.TryGetProperty("password", out var password)
                || username.ValueKind != JsonValueKind.String || password.ValueKind != JsonValueKind.String
                || string.IsNullOrWhiteSpace(username.GetString()) || string.IsNullOrWhiteSpace(password.GetString()))
                throw new InvalidOperationException("The database credentials secret requires username and password.");

            connection.Username = username.GetString();
            connection.Password = password.GetString();
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("The database credentials secret has an invalid format.");
        }
    }

    /// <summary>
    /// Validates that only the password changed between <paramref name="original"/> and
    /// <paramref name="refreshed"/> (all other settings must be identical) and returns the new password.
    /// Throws if the non-password settings diverged — that means a restart is required.
    /// </summary>
    public static string GetRefreshedPassword(
        NpgsqlConnectionStringBuilder original, NpgsqlConnectionStringBuilder refreshed)
    {
        var originalCopy = new NpgsqlConnectionStringBuilder(original.ConnectionString);
        var refreshedCopy = new NpgsqlConnectionStringBuilder(refreshed.ConnectionString);
        originalCopy.Remove("Password");
        refreshedCopy.Remove("Password");

        if (!originalCopy.EquivalentTo(refreshedCopy))
            throw new InvalidOperationException(
                "Database connection settings changed. Restart the API to load the new settings.");

        return refreshed.Password!;
    }
}
