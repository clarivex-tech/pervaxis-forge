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

using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Models.Responses;

namespace Pervaxis.Forge.Api.Services;

public interface IVerticalService
{
    Task<VerticalResponse> EnrollAsync(
        VerticalEnrollmentRequest request,
        CancellationToken ct = default);

    Task<IReadOnlyList<VerticalSummaryResponse>> ListAsync(CancellationToken ct = default);

    Task<VerticalResponse?> GetAsync(string slug, CancellationToken ct = default);

    Task<VerticalResponse?> UpdateAsync(
        string slug,
        UpdateVerticalRequest request,
        CancellationToken ct = default);

    Task<bool> UnenrollAsync(string slug, CancellationToken ct = default);
}
