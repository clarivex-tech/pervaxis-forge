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

using System.Text.RegularExpressions;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Engine.Naming;

namespace Pervaxis.Forge.Api.Services;

public static class VerticalRequestValidator
{
    private static readonly Regex SlugRegex = new("^[a-z][a-z0-9-]*$", RegexOptions.Compiled);
    private static readonly Regex EmailRegex = new("^\\S+@\\S+\\.\\S+$", RegexOptions.Compiled);
    private static readonly Regex AwsAccountIdRegex = new("^\\d{12}$", RegexOptions.Compiled);
    private static readonly Regex RegionRegex = new("^[a-z]{2}-[a-z]+-\\d$", RegexOptions.Compiled);
    private static readonly Regex GitHubOrgRegex = new("^\\S+$", RegexOptions.Compiled);
    private static readonly Regex KebabCaseRegex = new("^[a-z][a-z0-9-]*$", RegexOptions.Compiled);

    public static IReadOnlyList<ValidationFailure> Validate(VerticalEnrollmentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var failures = new List<ValidationFailure>();

        ValidateSlug(request.Slug, failures);
        ValidateDisplayName(request.DisplayName, failures);
        ValidateOwnerTeam(request.OwnerTeam, failures);
        ValidateEmail(request.OwnerEmail, failures);
        ValidateComponentPrefix(request.ComponentPrefix, failures);
        ValidateCloudProvider(request.CloudProvider, failures);
        ValidateSourceControl(request.SourceControl, failures);
        ValidateTechDefaults(request.TechDefaults, failures);

        return failures;
    }

    public static IReadOnlyList<ValidationFailure> Validate(UpdateVerticalRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var failures = new List<ValidationFailure>();

        ValidateDisplayName(request.DisplayName, failures);
        ValidateOwnerTeam(request.OwnerTeam, failures);
        ValidateEmail(request.OwnerEmail, failures);
        ValidateTechDefaults(request.TechDefaults, failures);

        return failures;
    }

    private static void ValidateSlug(string slug, ICollection<ValidationFailure> failures)
    {
        if (slug.Length is < 1 or > 100)
        {
            failures.Add(new ValidationFailure(nameof(VerticalEnrollmentRequest.Slug), "Slug must be between 1 and 100 characters."));
            return;
        }

        if (!SlugRegex.IsMatch(slug))
        {
            failures.Add(new ValidationFailure(nameof(VerticalEnrollmentRequest.Slug), "Slug must start with a lowercase letter and contain only lowercase letters, digits, and hyphens."));
        }

        if (slug.Contains("--", StringComparison.Ordinal))
        {
            failures.Add(new ValidationFailure(nameof(VerticalEnrollmentRequest.Slug), "Slug must not contain consecutive hyphens."));
        }

        if (slug.EndsWith("-", StringComparison.Ordinal))
        {
            failures.Add(new ValidationFailure(nameof(VerticalEnrollmentRequest.Slug), "Slug must not end with a hyphen."));
        }
    }

    private static void ValidateEmail(string email, ICollection<ValidationFailure> failures)
    {
        ValidateTrimmedLength(email, nameof(VerticalEnrollmentRequest.OwnerEmail), 1, 255, failures);

        if (!EmailRegex.IsMatch(email))
        {
            failures.Add(new ValidationFailure(nameof(VerticalEnrollmentRequest.OwnerEmail), "Owner email must be a valid email address."));
        }
    }

    private static void ValidateDisplayName(string displayName, ICollection<ValidationFailure> failures)
    {
        ValidateTrimmedLength(displayName, nameof(VerticalEnrollmentRequest.DisplayName), 1, 255, failures);
    }

    private static void ValidateOwnerTeam(string ownerTeam, ICollection<ValidationFailure> failures)
    {
        ValidateTrimmedLength(ownerTeam, nameof(VerticalEnrollmentRequest.OwnerTeam), 1, 255, failures);
    }

    private static void ValidateCloudProvider(CloudProviderConfig config, ICollection<ValidationFailure> failures)
    {
        if (!string.Equals(config.Provider, "AWS", StringComparison.Ordinal))
        {
            failures.Add(new ValidationFailure("CloudProvider.Provider", "Cloud provider must be AWS."));
        }

        if (!AwsAccountIdRegex.IsMatch(config.AwsAccountId))
        {
            failures.Add(new ValidationFailure("CloudProvider.AwsAccountId", "AWS account ID must be exactly 12 digits."));
        }

        if (config.IamRoleArn.Length > 2048 || !config.IamRoleArn.StartsWith("arn:aws:iam::", StringComparison.Ordinal) || !config.IamRoleArn.Contains(":role/", StringComparison.Ordinal))
        {
            failures.Add(new ValidationFailure("CloudProvider.IamRoleArn", "IAM role ARN must be a valid AWS IAM role ARN."));
        }

        if (!RegionRegex.IsMatch(config.DefaultRegion))
        {
            failures.Add(new ValidationFailure("CloudProvider.DefaultRegion", "Default region must be a valid AWS region code."));
        }
    }

    private static void ValidateSourceControl(SourceControlConfig config, ICollection<ValidationFailure> failures)
    {
        if (!string.Equals(config.Platform, "GitHub", StringComparison.Ordinal))
        {
            failures.Add(new ValidationFailure("SourceControl.Platform", "Source control platform must be GitHub."));
        }

        ValidateTrimmedLength(config.GitHubOrg, "SourceControl.GitHubOrg", 1, 255, failures);

        if (!GitHubOrgRegex.IsMatch(config.GitHubOrg))
        {
            failures.Add(new ValidationFailure("SourceControl.GitHubOrg", "GitHub org must not contain whitespace."));
        }

        ValidateTrimmedLength(config.AccessToken, "SourceControl.AccessToken", 1, 512, failures);

        if (!string.Equals(config.DefaultVisibility, "Private", StringComparison.Ordinal) &&
            !string.Equals(config.DefaultVisibility, "Public", StringComparison.Ordinal))
        {
            failures.Add(new ValidationFailure("SourceControl.DefaultVisibility", "Default visibility must be Private or Public."));
        }
    }

    private static void ValidateTechDefaults(VerticalTechDefaults techDefaults, ICollection<ValidationFailure> failures)
    {
        if (techDefaults.Environments.Count == 0)
        {
            failures.Add(new ValidationFailure("TechDefaults.Environments", "At least one environment is required."));
            return;
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        foreach (var environment in techDefaults.Environments)
        {
            if (!KebabCaseRegex.IsMatch(environment))
            {
                failures.Add(new ValidationFailure("TechDefaults.Environments", $"Environment '{environment}' must be kebab-case."));
            }

            if (!seen.Add(environment))
            {
                failures.Add(new ValidationFailure("TechDefaults.Environments", $"Environment '{environment}' must be unique."));
            }
        }

        if (!techDefaults.Environments.Contains(techDefaults.DefaultEnvironment, StringComparer.Ordinal))
        {
            failures.Add(new ValidationFailure("TechDefaults.DefaultEnvironment", "Default environment must be one of the environments."));
        }

        if (techDefaults.DefaultDbEngine is not null &&
            techDefaults.DefaultDbEngine is not ("postgresql" or "mysql" or "none"))
        {
            failures.Add(new ValidationFailure("TechDefaults.DefaultDbEngine", "Default database engine must be postgresql, mysql, or none."));
        }
    }

    private static void ValidateTrimmedLength(
        string value,
        string field,
        int minLength,
        int maxLength,
        ICollection<ValidationFailure> failures)
    {
        var trimmed = value.Trim();
        if (trimmed.Length < minLength || trimmed.Length > maxLength)
        {
            failures.Add(new ValidationFailure(field, $"{field} must be between {minLength} and {maxLength} characters after trimming."));
        }
    }

    private static void ValidateComponentPrefix(string prefix, ICollection<ValidationFailure> failures)
    {
        try
        {
            NamingConvention.GetComponentPrefix(prefix);
        }
        catch (ArgumentException ex)
        {
            failures.Add(new ValidationFailure(nameof(VerticalEnrollmentRequest.ComponentPrefix), ex.Message));
        }
    }
}
