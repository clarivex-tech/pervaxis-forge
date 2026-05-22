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

using System.IO.Compression;
using FluentAssertions;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Pervaxis.Forge.Engine.Validation;
using Xunit;

namespace Pervaxis.Forge.Engine.Tests.Generation;

public class AngularGenerationTests
{
    private readonly ManifestValidator validator = new();
    private readonly PrintGenerator generator = new();

    [Fact]
    public async Task GenerateAsync_AngularShell_WithCanvasModules_ReturnsZip()
    {
        var manifest = new ForgeManifest
        {
            Product = "mat",
            VerticalSlug = "matrimony",
            ServiceName = "matr-shell",
            ServiceType = ServiceType.AngularShell,
            ComponentPrefix = "mat",
            CloudProvider = "AWS",
            GenesisModules = [],
            CanvasModules = ["Workspace", "Shell", "Layout", "Navigation", "Auth"],
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        var entries = archive.Entries.Select(e => e.FullName).ToList();
        entries.Should().Contain("package.json");
        entries.Should().Contain("angular.json");
        entries.Should().Contain("apps/matr-shell/src/app/app.config.ts");
        entries.Should().Contain("apps/matr-shell/src/app/app.component.ts");
        entries.Should().Contain("apps/matr-shell/src/app/app.routes.ts");

        using var configStream = new StreamReader(archive.GetEntry("apps/matr-shell/src/app/app.config.ts")!.Open());
        var configContent = await configStream.ReadToEndAsync();
        configContent.Should().Contain("CanvasNavigationModule");
        configContent.Should().Contain("@pervaxis/canvas-navigation");
    }

    [Fact]
    public async Task GenerateAsync_AngularMfe_WithCanvasModules_ReturnsZip()
    {
        var manifest = new ForgeManifest
        {
            Product = "mat",
            VerticalSlug = "matrimony",
            ServiceName = "intake-profile",
            ServiceType = ServiceType.AngularMfe,
            ComponentPrefix = "mat",
            CloudProvider = "AWS",
            GenesisModules = [],
            CanvasModules = ["Dashboard", "Notifications"],
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        var entries = archive.Entries.Select(e => e.FullName).ToList();
        entries.Should().Contain("libs/intake-profile/src/lib/routes.ts");
        entries.Should().Contain("libs/intake-profile/src/lib/component.ts");
        entries.Should().Contain("libs/intake-profile/src/index.ts");

        using var componentStream = new StreamReader(archive.GetEntry("libs/intake-profile/src/lib/component.ts")!.Open());
        var componentContent = await componentStream.ReadToEndAsync();
        componentContent.Should().Contain("CanvasDashboardModule");
        componentContent.Should().Contain("@pervaxis/canvas-dashboard");
    }

    [Fact]
    public async Task GenerateAsync_AngularMfeRemote_WithCanvasModules_ReturnsZip()
    {
        var manifest = new ForgeManifest
        {
            Product = "mat",
            VerticalSlug = "matrimony",
            ServiceName = "intake-profile",
            ServiceType = ServiceType.AngularMfeRemote,
            ComponentPrefix = "mat",
            CloudProvider = "AWS",
            CanvasModules = ["Dashboard"],
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        archive.Entries.Select(e => e.FullName).Should().Contain("libs/intake-profile/src/lib/component.ts");
    }

    [Fact]
    public async Task GenerateAsync_Monolithic_ReturnsZip()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-web",
            ServiceType = ServiceType.Monolithic,
            ComponentPrefix = "clv",
            CloudProvider = "AWS",
            UiTargets = ["web", "mobile"],
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        archive.Entries.Select(e => e.FullName).Should().Contain("manifest.json");
    }

    [Fact]
    public async Task GenerateAsync_Ionic_ReturnsZip()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-mobile",
            ServiceType = ServiceType.Ionic,
            ComponentPrefix = "clv",
            CloudProvider = "AWS",
            UiTargets = ["mobile"],
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        archive.Entries.Select(e => e.FullName).Should().Contain("manifest.json");
    }

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

    [Fact]
    public void Validate_AcceptsMonolithicWithWebAndMobileTargets()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-web",
            ServiceType = ServiceType.Monolithic,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            UiTargets = ["web", "mobile"],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_AcceptsMonolithicWithWebOnlyTarget()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-web",
            ServiceType = ServiceType.Monolithic,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            UiTargets = ["web"],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_RejectsAngularShellWithUiTargets()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-shell",
            ServiceType = ServiceType.AngularShell,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            UiTargets = ["web"],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("must not include uiTargets"));
    }

    [Fact]
    public void Validate_RejectsIonicWithoutMobileTarget()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "claims-mobile",
            ServiceType = ServiceType.Ionic,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            UiTargets = ["web"],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("Ionic requests must use uiTargets"));
    }
}
