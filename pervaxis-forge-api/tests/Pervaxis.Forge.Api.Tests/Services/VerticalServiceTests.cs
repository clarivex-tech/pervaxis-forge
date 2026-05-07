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
using Pervaxis.Forge.Api.Data.Entities;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Services;
using Xunit;

using EntityTechDefaults = Pervaxis.Forge.Api.Data.Entities.VerticalTechDefaults;
using RequestTechDefaults = Pervaxis.Forge.Api.Models.Requests.VerticalTechDefaults;

namespace Pervaxis.Forge.Api.Tests.Services;

public class VerticalServiceTests
{
    // Smoke test — verifies the models assembly loads and request records are constructable.
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
            TechDefaults = new RequestTechDefaults
            {
                DefaultEnvironment = "test"
            }
        };

        request.Slug.Should().Be("clarivolt");
        request.CloudProvider.Provider.Should().Be("AWS");
        request.TechDefaults.Environments.Should().BeEquivalentTo(["test", "accp", "prod"]);
    }

    [Fact]
    public void MapToResponse_ProjectsAllFields_FromVerticalAggregate()
    {
        var vertical = new Vertical
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            Slug = "clarivolt",
            DisplayName = "Clarivolt",
            Description = "Sales platform",
            OwnerTeam = "Platform Team",
            OwnerEmail = "team@clarivex.tech",
            CreatedAt = new DateTime(2026, 5, 7, 10, 0, 0, DateTimeKind.Utc),
            CloudConfig = new VerticalCloudConfig
            {
                Provider = "AWS",
                AwsAccountId = "123456789012",
                IamRoleArn = "arn:aws:iam::123456789012:role/ForgeDeploymentRole",
                DefaultRegion = "us-east-1",
            },
            SourceControlConfig = new VerticalSourceControlConfig
            {
                Platform = "GitHub",
                GitHubOrg = "clarivex-tech",
                AccessToken = "ghp_test",
                DefaultVisibility = "Private",
                DefaultBranchProtection = true,
            },
            TechDefaults = new EntityTechDefaults
            {
                Environments = ["test", "accp", "prod"],
                DefaultEnvironment = "test",
            },
        };

        var response = VerticalService.MapToResponse(vertical);

        response.Id.Should().Be(vertical.Id);
        response.Slug.Should().Be("clarivolt");
        response.DisplayName.Should().Be("Clarivolt");
        response.CloudProvider.Should().Be("AWS");
        response.SourceControl.Should().Be("GitHub");
        response.GitHubOrg.Should().Be("clarivex-tech");
        response.Environments.Should().BeEquivalentTo(["test", "accp", "prod"]);
        response.EnrolledAt.Should().Be(new DateTimeOffset(2026, 5, 7, 10, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public void MapToResponse_FallsBackToDefaults_WhenChildConfigsAreMissing()
    {
        var vertical = new Vertical
        {
            Slug = "bare",
            DisplayName = "Bare",
            OwnerTeam = "T",
            OwnerEmail = "t@e.com",
            CreatedAt = new DateTime(2026, 5, 7, 0, 0, 0, DateTimeKind.Utc),
        };

        var response = VerticalService.MapToResponse(vertical);

        response.CloudProvider.Should().Be("Unknown");
        response.SourceControl.Should().Be("Unknown");
        response.GitHubOrg.Should().BeEmpty();
        response.Environments.Should().BeEmpty();
    }
}
