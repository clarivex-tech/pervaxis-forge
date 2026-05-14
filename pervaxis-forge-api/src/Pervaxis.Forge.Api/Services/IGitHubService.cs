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

namespace Pervaxis.Forge.Api.Services;

public interface IGitHubService
{
    Task<string> CreateRepositoryAsync(string accessToken, string orgName, string repoName, string description, bool isPrivate, CancellationToken ct = default);
    Task ConfigureBranchProtectionAsync(string accessToken, string orgName, string repoName, string branch, CancellationToken ct = default);
    Task PushInitialCommitAsync(string cloneUrl, string accessToken, byte[] scaffoldZip, string authorName, string authorEmail, CancellationToken ct = default);
}
