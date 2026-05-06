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
using Xunit;

namespace Pervaxis.Forge.Api.Tests.Services;

public class VerticalServiceTests
{
    // Smoke test — verifies the models assembly loads and request records are constructable.
    // Full unit tests (mocked DB, encryption verification) come once VerticalService is implemented.
    [Fact]
    public void VerticalEnrollmentRequest_CanBeConstructed_WithRequiredProperties()
    {
        var request = new VerticalEnrollmentRequest
        {
            Slug = "clarivolt",
            DisplayName = "Clarivolt",
            Description = "Sales upload and validation platform",
            OwnerTeam = "Clarivolt Platform Team",
            OwnerEmail = "team@clarivex.tech",
            CloudProvider = new CloudProviderConfig
            {
                Provider = "AWS",
                AwsAccountId = "123456789012",
                IamRoleArn = "arn:aws:iam::123456789012:role/ForgeDeploymentRole",
                DefaultRegion = "us-east-1"
            },
            SourceControl = new SourceControlConfig
            {
                Platform = "GitHub",
                GitHubOrg = "clarivex-tech",
                AccessToken = "ghp_test",
                DefaultVisibility = "Private",
                DefaultBranchProtection = true
            },
            TechDefaults = new VerticalTechDefaults
            {
                DefaultEnvironment = "test"
            }
        };

        request.Slug.Should().Be("clarivolt");
        request.CloudProvider.Provider.Should().Be("AWS");
        request.TechDefaults.Environments.Should().BeEquivalentTo(["test", "accp", "prod"]);
    }
}
