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

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Microsoft.EntityFrameworkCore;
using Octokit;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Models.Responses;

namespace Pervaxis.Forge.Api.Services;

public sealed class VerticalConnectivityValidator(
    ForgeDbContext db,
    IAmazonSecurityTokenService sts,
    Func<string, IGitHubClient> gitHubClientFactory) : IVerticalConnectivityValidator
{
    public async Task<ConnectivityValidationResponse?> ValidateAsync(string slug, CancellationToken ct = default)
    {
        var vertical = await db.Verticals
            .Include(v => v.CloudConfig)
            .Include(v => v.SourceControlConfig)
            .FirstOrDefaultAsync(v => v.Slug == slug && v.IsActive, ct);

        if (vertical is null) return null;

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));

        var awsTask = CheckAwsAsync(vertical.CloudConfig?.IamRoleArn ?? string.Empty, vertical.CloudConfig?.AwsAccountId ?? string.Empty, timeout.Token);
        var gitHubTask = CheckGitHubAsync(vertical.SourceControlConfig?.AccessToken ?? string.Empty, vertical.SourceControlConfig?.GitHubOrg ?? string.Empty, timeout.Token);

        await Task.WhenAll(awsTask, gitHubTask);

        return new ConnectivityValidationResponse
        {
            AwsConnectivity = awsTask.Result,
            GitHubConnectivity = gitHubTask.Result,
        };
    }

    private async Task<AwsConnectivityResult> CheckAwsAsync(string roleArn, string accountId, CancellationToken ct)
    {
        try
        {
            await sts.AssumeRoleAsync(new AssumeRoleRequest
            {
                RoleArn = roleArn,
                RoleSessionName = "forge-connectivity-check",
                DurationSeconds = 900,
            }, ct);

            return new AwsConnectivityResult { Success = true, AccountId = accountId };
        }
        catch (Exception ex)
        {
            return new AwsConnectivityResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    private async Task<GitHubConnectivityResult> CheckGitHubAsync(string accessToken, string org, CancellationToken ct)
    {
        try
        {
            var client = gitHubClientFactory(accessToken);
            var organization = await client.Organization.Get(org);
            return new GitHubConnectivityResult { Success = true, Org = organization.Login };
        }
        catch (Exception ex)
        {
            return new GitHubConnectivityResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
