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
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Validation;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public class AngularGenerationTests
{
    private readonly ManifestValidator validator = new();

    [Fact]
    public void Validate_RejectsAngularShellNotEndingWithShell()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-overview",
            ServiceType = ServiceType.AngularShell,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = [],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Angular Shell") && error.Contains("-shell"));
    }

    [Fact]
    public void Validate_AcceptsValidAngularShellNameEndingWithShell()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-shell",
            ServiceType = ServiceType.AngularShell,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = [],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_RejectsAngularMfeNameEndingWithShell()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-shell",
            ServiceType = ServiceType.AngularMfe,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = [],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Angular MFE") && error.Contains("-shell"));
    }

    [Fact]
    public void Validate_AcceptsValidAngularMfeNameWithoutShellOrServiceSuffix()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-overview",
            ServiceType = ServiceType.AngularMfe,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = [],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
