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

using FluentAssertions;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Services;
using Xunit;

namespace Pervaxis.Forge.Api.Tests.Services;

public class VerticalRequestValidatorTests
{
    [Theory]
    [InlineData("clarivolt", true)]
    [InlineData("intake-service", true)]
    [InlineData("a", true)]
    [InlineData("Bad_Slug", false)]
    [InlineData("bad--slug", false)]
    public void ValidateSlug_Rules_AreEnforced(string slug, bool isValid)
    {
        var request = CreateRequest(slug: slug);

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == nameof(VerticalEnrollmentRequest.Slug)).Should().Be(!isValid);
    }

    [Theory]
    [InlineData("Clarivolt", true)]
    [InlineData("  Clarivolt  ", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void ValidateUpdateDisplayName_TrimmedLength_IsEnforced(string? displayName, bool isValid)
    {
        var request = CreateUpdateRequest(displayName: displayName);

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == nameof(VerticalEnrollmentRequest.DisplayName)).Should().Be(!isValid);
    }

    [Theory]
    [InlineData("Platform Team", true)]
    [InlineData("  Platform Team  ", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void ValidateUpdateOwnerTeam_TrimmedLength_IsEnforced(string? ownerTeam, bool isValid)
    {
        var request = CreateUpdateRequest(ownerTeam: ownerTeam);

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == nameof(VerticalEnrollmentRequest.OwnerTeam)).Should().Be(!isValid);
    }

    [Theory]
    [InlineData("team@clarivex.tech", true)]
    [InlineData(" team@clarivex.tech ", false)]
    [InlineData("not-an-email", false)]
    [InlineData("team@", false)]
    [InlineData("", false)]
    public void ValidateUpdateOwnerEmail_Rules_AreEnforced(string ownerEmail, bool isValid)
    {
        var request = CreateUpdateRequest(ownerEmail: ownerEmail);

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == nameof(VerticalEnrollmentRequest.OwnerEmail)).Should().Be(!isValid);
    }

    [Theory]
    [InlineData("Clarivolt", true)]
    [InlineData("  Clarivolt  ", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void ValidateDisplayName_TrimmedLength_IsEnforced(string? displayName, bool isValid)
    {
        var request = CreateRequest(displayName: displayName);

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == nameof(VerticalEnrollmentRequest.DisplayName)).Should().Be(!isValid);
    }

    [Theory]
    [InlineData("Platform Team", true)]
    [InlineData("  Platform Team  ", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void ValidateOwnerTeam_TrimmedLength_IsEnforced(string? ownerTeam, bool isValid)
    {
        var request = CreateRequest(ownerTeam: ownerTeam);

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == nameof(VerticalEnrollmentRequest.OwnerTeam)).Should().Be(!isValid);
    }

    [Theory]
    [InlineData("team@clarivex.tech", true)]
    [InlineData(" team@clarivex.tech ", false)]
    [InlineData("not-an-email", false)]
    [InlineData("team@", false)]
    [InlineData("", false)]
    public void ValidateOwnerEmail_Rules_AreEnforced(string ownerEmail, bool isValid)
    {
        var request = CreateRequest(ownerEmail: ownerEmail);

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == nameof(VerticalEnrollmentRequest.OwnerEmail)).Should().Be(!isValid);
    }

    [Theory]
    [InlineData("AWS", true)]
    [InlineData("GCP", false)]
    [InlineData("aws", false)]
    public void ValidateCloudProviderProvider_Rules_AreEnforced(string provider, bool isValid)
    {
        var request = CreateRequest(cloudProvider: CreateCloudProvider(provider: provider));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "CloudProvider.Provider").Should().Be(!isValid);
    }

    [Theory]
    [InlineData("123456789012", true)]
    [InlineData("12345678901", false)]
    [InlineData("12345678901a", false)]
    public void ValidateCloudProviderAwsAccountId_Rules_AreEnforced(string accountId, bool isValid)
    {
        var request = CreateRequest(cloudProvider: CreateCloudProvider(awsAccountId: accountId));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "CloudProvider.AwsAccountId").Should().Be(!isValid);
    }

    [Theory]
    [InlineData("arn:aws:iam::123456789012:role/ForgeDeploymentRole", true)]
    [InlineData("arn:aws:iam::123456789012:user/ForgeDeploymentRole", false)]
    [InlineData("arn:aws:iam::123456789012:role/", true)]
    [InlineData("arn:aws:s3::123456789012:role/ForgeDeploymentRole", false)]
    [InlineData("", false)]
    public void ValidateCloudProviderIamRoleArn_Rules_AreEnforced(string arn, bool isValid)
    {
        var request = CreateRequest(cloudProvider: CreateCloudProvider(iamRoleArn: arn));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "CloudProvider.IamRoleArn").Should().Be(!isValid);
    }

    [Theory]
    [InlineData("us-east-1", true)]
    [InlineData("eu-west-1", true)]
    [InlineData("useast1", false)]
    [InlineData("us-east-10", false)]
    [InlineData("US-EAST-1", false)]
    public void ValidateCloudProviderDefaultRegion_Rules_AreEnforced(string region, bool isValid)
    {
        var request = CreateRequest(cloudProvider: CreateCloudProvider(defaultRegion: region));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "CloudProvider.DefaultRegion").Should().Be(!isValid);
    }

    [Theory]
    [InlineData("GitHub", true)]
    [InlineData("Gitlab", false)]
    [InlineData("github", false)]
    public void ValidateSourceControlPlatform_Rules_AreEnforced(string platform, bool isValid)
    {
        var request = CreateRequest(sourceControl: CreateSourceControl(platform: platform));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "SourceControl.Platform").Should().Be(!isValid);
    }

    [Theory]
    [InlineData("clarivex-tech", true)]
    [InlineData(" clarivex-tech ", false)]
    [InlineData("clarivex tech", false)]
    [InlineData("", false)]
    public void ValidateSourceControlGitHubOrg_Rules_AreEnforced(string githubOrg, bool isValid)
    {
        var request = CreateRequest(sourceControl: CreateSourceControl(gitHubOrg: githubOrg));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "SourceControl.GitHubOrg").Should().Be(!isValid);
    }

    [Theory]
    [InlineData("ghp_test", true)]
    [InlineData("   ghp_test   ", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void ValidateSourceControlAccessToken_TrimmedLength_IsEnforced(string accessToken, bool isValid)
    {
        var request = CreateRequest(sourceControl: CreateSourceControl(accessToken: accessToken));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "SourceControl.AccessToken").Should().Be(!isValid);
    }

    [Theory]
    [InlineData("Private", true)]
    [InlineData("Public", true)]
    [InlineData("Internal", false)]
    public void ValidateSourceControlDefaultVisibility_Rules_AreEnforced(string visibility, bool isValid)
    {
        var request = CreateRequest(sourceControl: CreateSourceControl(defaultVisibility: visibility));

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "SourceControl.DefaultVisibility").Should().Be(!isValid);
    }

    [Fact]
    public void ValidateTechDefaults_RejectsEmptyEnvironments()
    {
        var request = CreateRequest(techDefaults: new VerticalTechDefaults
        {
            Environments = [],
            DefaultEnvironment = "test"
        });

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "TechDefaults.Environments").Should().BeTrue();
    }

    [Fact]
    public void ValidateTechDefaults_RejectsDuplicateEnvironments()
    {
        var request = CreateRequest(techDefaults: new VerticalTechDefaults
        {
            Environments = ["test", "test"],
            DefaultEnvironment = "test"
        });

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "TechDefaults.Environments" && f.Message.Contains("unique", StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    [Fact]
    public void ValidateTechDefaults_DefaultEnvironment_MustBeInEnvironments()
    {
        var request = CreateRequest(techDefaults: new VerticalTechDefaults
        {
            Environments = ["test", "accp"],
            DefaultEnvironment = "prod"
        });

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "TechDefaults.DefaultEnvironment").Should().BeTrue();
    }

    [Theory]
    [InlineData("postgresql", true)]
    [InlineData("mysql", true)]
    [InlineData("none", true)]
    [InlineData("sqlite", false)]
    public void ValidateTechDefaults_DefaultDbEngine_Rules_AreEnforced(string? engine, bool isValid)
    {
        var request = CreateRequest(techDefaults: new VerticalTechDefaults
        {
            Environments = ["test", "accp"],
            DefaultEnvironment = "test",
            DefaultDbEngine = engine
        });

        var failures = VerticalRequestValidator.Validate(request);

        failures.Any(f => f.Field == "TechDefaults.DefaultDbEngine").Should().Be(!isValid);
    }

    private static VerticalEnrollmentRequest CreateRequest(
        string? slug = null,
        string? displayName = null,
        string? ownerTeam = null,
        string? ownerEmail = null,
        CloudProviderConfig? cloudProvider = null,
        SourceControlConfig? sourceControl = null,
        VerticalTechDefaults? techDefaults = null)
    {
        return new VerticalEnrollmentRequest
        {
            Slug = slug ?? "clarivolt",
            DisplayName = displayName ?? "Clarivolt",
            Description = "Sales platform",
            OwnerTeam = ownerTeam ?? "Platform Team",
            OwnerEmail = ownerEmail ?? "team@clarivex.tech",
            CloudProvider = cloudProvider ?? CreateCloudProvider(),
            SourceControl = sourceControl ?? CreateSourceControl(),
            TechDefaults = techDefaults ?? new VerticalTechDefaults
            {
                Environments = ["test", "accp", "prod"],
                DefaultEnvironment = "test"
            }
        };
    }

    private static UpdateVerticalRequest CreateUpdateRequest(
        string? displayName = null,
        string? ownerTeam = null,
        string? ownerEmail = null,
        VerticalTechDefaults? techDefaults = null)
    {
        return new UpdateVerticalRequest
        {
            DisplayName = displayName ?? "Clarivolt",
            Description = "Sales platform",
            OwnerTeam = ownerTeam ?? "Platform Team",
            OwnerEmail = ownerEmail ?? "team@clarivex.tech",
            TechDefaults = techDefaults ?? new VerticalTechDefaults
            {
                Environments = ["test", "accp", "prod"],
                DefaultEnvironment = "test"
            }
        };
    }

    private static CloudProviderConfig CreateCloudProvider(
        string? provider = null,
        string? awsAccountId = null,
        string? iamRoleArn = null,
        string? defaultRegion = null)
    {
        return new CloudProviderConfig
        {
            Provider = provider ?? "AWS",
            AwsAccountId = awsAccountId ?? "123456789012",
            IamRoleArn = iamRoleArn ?? "arn:aws:iam::123456789012:role/ForgeDeploymentRole",
            DefaultRegion = defaultRegion ?? "us-east-1"
        };
    }

    private static SourceControlConfig CreateSourceControl(
        string? platform = null,
        string? gitHubOrg = null,
        string? accessToken = null,
        string? defaultVisibility = null)
    {
        return new SourceControlConfig
        {
            Platform = platform ?? "GitHub",
            GitHubOrg = gitHubOrg ?? "clarivex-tech",
            AccessToken = accessToken ?? "ghp_test",
            DefaultVisibility = defaultVisibility ?? "Private",
            DefaultBranchProtection = true
        };
    }
}
