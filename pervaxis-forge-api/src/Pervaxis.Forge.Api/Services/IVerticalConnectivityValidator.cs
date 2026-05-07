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

using Pervaxis.Forge.Api.Models.Responses;

namespace Pervaxis.Forge.Api.Services;

public interface IVerticalConnectivityValidator
{
    /// <summary>
    /// Validates that a vertical's stored AWS and GitHub credentials actually work.
    /// </summary>
    /// <param name="slug">The vertical slug to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A ConnectivityValidationResponse with results, or null when the slug is not found.</returns>
    Task<ConnectivityValidationResponse?> ValidateAsync(string slug, CancellationToken ct = default);
}
