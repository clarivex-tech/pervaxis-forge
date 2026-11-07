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

using System.Data.Common;
using Npgsql;

namespace Pervaxis.Forge.Api.Infrastructure.Database;

/// <summary>
/// Opens database connections with automatic credential rotation on authentication failure.
///
/// When Postgres returns error 28P01 (invalid password), the opener fetches fresh credentials
/// from AWS Secrets Manager and retries the connection once. Concurrent failures are coalesced
/// into a single Secrets Manager call using a SemaphoreSlim generation counter, and refresh
/// attempts are throttled to at most once every 5 seconds.
/// </summary>
public sealed class DatabaseConnectionOpener(
    NpgsqlDataSource dataSource,
    Func<CancellationToken, Task<NpgsqlConnectionStringBuilder>>? refreshCredentials,
    TimeProvider? timeProvider = null) : IDisposable
{
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly TimeProvider _clock = timeProvider ?? TimeProvider.System;
    private readonly NpgsqlConnectionStringBuilder _settings = new(dataSource.ConnectionString);
    private DateTimeOffset _nextRefreshAllowed = DateTimeOffset.MinValue;
    private long _generation;

    public async Task OpenAsync(DbConnection connection, CancellationToken cancellationToken = default)
    {
        var generation = Interlocked.Read(ref _generation);
        try
        {
            await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
            return;
        }
        catch (PostgresException ex) when (
            ex.SqlState == PostgresErrorCodes.InvalidPassword && refreshCredentials is not null)
        {
            if (!await RefreshAsync(generation, cancellationToken).ConfigureAwait(false))
                throw;
        }

        // A second failure propagates. Do not replay commands or transactions.
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task<bool> RefreshAsync(long failedGeneration, CancellationToken cancellationToken)
    {
        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            // Another caller already refreshed the password used by this failing generation.
            if (failedGeneration != Interlocked.Read(ref _generation))
                return true;

            if (_clock.GetUtcNow() < _nextRefreshAllowed)
                return false;

            try
            {
                var updated = await refreshCredentials!(cancellationToken).ConfigureAwait(false);
                dataSource.Password = DatabaseSecretReader.GetRefreshedPassword(_settings, updated);
                Interlocked.Increment(ref _generation);
                return true;
            }
            finally
            {
                // Throttle even failed or unchanged lookups.
                _nextRefreshAllowed = _clock.GetUtcNow().AddSeconds(5);
            }
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void Dispose() => _refreshLock.Dispose();
}
