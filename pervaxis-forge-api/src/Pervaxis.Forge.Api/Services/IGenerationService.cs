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

public interface IGenerationService
{
    Task<(byte[] Zip, GenerationResult Result)> GenerateAsync(GenerationRequest request, string generatedBy, CancellationToken ct = default);
    Task<(byte[] Zip, BatchGenerationResult Result)> GenerateBatchAsync(BatchGenerationRequest request, CancellationToken ct = default);
    Task<ValidationPreviewResult> ValidateAsync(GenerationRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<GenerationAuditEntry>> GetAuditLogAsync(string verticalSlug, CancellationToken ct = default);
    Task<IReadOnlyList<GeneratedServiceResponse>> ListGeneratedServicesAsync(string verticalSlug, CancellationToken ct = default);
    Task<(byte[] Zip, GeneratedServiceResponse Service)> RegenerateAsync(string verticalSlug, Guid serviceId, CancellationToken ct = default);
}
