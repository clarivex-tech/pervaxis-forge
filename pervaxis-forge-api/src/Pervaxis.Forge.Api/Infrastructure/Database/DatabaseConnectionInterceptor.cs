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
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Pervaxis.Forge.Api.Infrastructure.Database;

/// <summary>
/// EF Core interceptor that delegates connection opening to <see cref="DatabaseConnectionOpener"/>,
/// enabling transparent AWS RDS credential rotation on Postgres 28P01 auth failures.
/// </summary>
public sealed class DatabaseConnectionInterceptor(DatabaseConnectionOpener opener) : DbConnectionInterceptor
{
    public override InterceptionResult ConnectionOpening(
        DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
    {
        if (result.IsSuppressed)
            return result;

        opener.OpenAsync(connection).GetAwaiter().GetResult();
        return InterceptionResult.Suppress();
    }

    public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(
        DbConnection connection, ConnectionEventData eventData, InterceptionResult result,
        CancellationToken cancellationToken = default)
    {
        if (result.IsSuppressed)
            return result;

        await opener.OpenAsync(connection, cancellationToken).ConfigureAwait(false);
        return InterceptionResult.Suppress();
    }
}
