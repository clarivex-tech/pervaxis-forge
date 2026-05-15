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

public class GrpcGenerationTests
{
    private readonly PrintGenerator generator = new();
    private readonly ManifestValidator validator = new();

    [Fact]
    public async Task GenerateAsync_GrpcGeneratesNonEmptyZipWithManifest()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "events-service",
            ServiceType = ServiceType.Grpc,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = ["Messaging"],
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        archive.Entries.Should().NotBeEmpty();
        archive.Entries.Should().Contain(e => e.FullName == "manifest.json");
    }

    [Fact]
    public async Task GenerateAsync_GrpcZipContainsExpectedFiles()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "events-service",
            ServiceType = ServiceType.Grpc,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = ["Messaging"],
        };

        var zipBytes = await generator.GenerateAsync(manifest, "AWS");

        using var archive = new ZipArchive(new MemoryStream(zipBytes), ZipArchiveMode.Read);
        var entries = archive.Entries.Select(e => e.FullName).ToList();

        entries.Should().Contain("manifest.json");
        entries.Should().Contain("src/Pervaxis.Clarivolt.Events/Program.cs");
        entries.Should().Contain("src/Pervaxis.Clarivolt.Events/ServiceImpl.cs");
        entries.Should().Contain("src/Pervaxis.Clarivolt.Events/Protos/events-service.proto");
        entries.Should().Contain("Dockerfile");
    }

    [Fact]
    public void Validate_RejectsGrpcNameNotEndingWithService()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "events-api",
            ServiceType = ServiceType.Grpc,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = ["Messaging"],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.Contains("gRPC") && error.Contains("-service"));
    }

    [Fact]
    public void Validate_AcceptsValidGrpcNameEndingWithService()
    {
        var manifest = new ForgeManifest
        {
            Product = "clarivolt",
            VerticalSlug = "clarivolt",
            ServiceName = "events-service",
            ServiceType = ServiceType.Grpc,
            ComponentPrefix = "CLV",
            CloudProvider = "AWS",
            GenesisModules = ["Messaging"],
        };

        var result = validator.Validate(manifest);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
