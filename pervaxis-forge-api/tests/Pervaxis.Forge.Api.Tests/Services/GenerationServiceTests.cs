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
using Microsoft.EntityFrameworkCore;
using Moq;
using Pervaxis.Forge.Api.Data;
using Pervaxis.Forge.Api.Data.Entities;
using Pervaxis.Forge.Api.Models.Requests;
using Pervaxis.Forge.Api.Models.Responses;
using Pervaxis.Forge.Api.Services;
using Pervaxis.Forge.Engine.Generation;
using Pervaxis.Forge.Engine.Manifest;
using Xunit;

namespace Pervaxis.Forge.Api.Tests.Services;

public sealed class GenerationServiceTests
{
    [Fact]
    public async Task GenerateAsync_RejectsDuplicateServiceNameWithinVertical()
    {
        await using var fixture = await TestDb.CreateAsync();
        fixture.SeedVerticalWithGeneratedService("clarivolt", "billing-service");

        var service = CreateService(fixture.Db);
        var request = CreateRequest("clarivolt", "billing-service");

        var action = () => service.GenerateAsync(request, "api-user");

        await action.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task ListGeneratedServicesAsync_ReturnsPersistedServices()
    {
        await using var fixture = await TestDb.CreateAsync();
        fixture.SeedVerticalWithGeneratedService("clarivolt", "billing-service");

        var service = CreateService(fixture.Db);

        var services = await service.ListGeneratedServicesAsync("clarivolt");

        services.Should().HaveCount(1);
        services[0].ServiceName.Should().Be("billing-service");
        services[0].ServiceType.Should().Be("RestApi");
        services[0].CloudProvider.Should().Be("AWS");
        services[0].GeneratedBy.Should().Be("forge-api");
    }

    [Fact]
    public async Task RegenerateAsync_RebuildsZipFromStoredManifest()
    {
        await using var fixture = await TestDb.CreateAsync();
        var serviceId = fixture.SeedVerticalWithGeneratedService("clarivolt", "billing-service");

        var service = CreateService(fixture.Db);

        var (zip, regenerated) = await service.RegenerateAsync("clarivolt", serviceId);

        zip.Should().NotBeEmpty();
        regenerated.ServiceName.Should().Be("billing-service");
        regenerated.GeneratedBy.Should().Be("forge-api");
    }

    private static GenerationService CreateService(ForgeDbContext db)
    {
        var verticalService = new Mock<IVerticalService>();
        verticalService.Setup(v => v.GetAsync("clarivolt", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new VerticalResponse
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Slug = "clarivolt",
                DisplayName = "Clarivolt",
                CloudProvider = "AWS",
                SourceControl = "GitHub",
                GitHubOrg = "clarivex-tech",
                Environments = ["test"],
                EnrolledAt = DateTimeOffset.UtcNow,
                ComponentPrefix = "clv"
            });

        return new GenerationService(
            db,
            verticalService.Object,
            new PrintGenerator(),
            Mock.Of<IGitHubService>());
    }

    private static GenerationRequest CreateRequest(string verticalSlug, string name) => new()
    {
        VerticalSlug = verticalSlug,
        Name = name,
        DisplayName = "Billing Service",
        Description = "Billing scaffold",
        Version = "1.0.0",
        Type = "RestApi",
        GenesisModules = ["Messaging"],
        Database = new GenerationDatabaseConfig
        {
            Engine = "PostgreSQL",
            Schema = "billing"
        },
        Metadata = new GenerationMetadata()
    };

    private sealed class TestDb : IAsyncDisposable
    {
        public ForgeDbContext Db { get; }

        private TestDb(ForgeDbContext db)
        {
            Db = db;
        }

        public static async Task<TestDb> CreateAsync()
        {
            var options = new DbContextOptionsBuilder<ForgeDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var db = new ForgeDbContext(options);
            await db.Database.EnsureCreatedAsync();

            return new TestDb(db);
        }

        public Guid SeedVerticalWithGeneratedService(string slug, string serviceName)
        {
            var verticalId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Db.Verticals.Add(new Vertical
            {
                Id = verticalId,
                Slug = slug,
                DisplayName = "Clarivolt",
                OwnerTeam = "Platform",
                OwnerEmail = "team@clarivex.tech",
                ComponentPrefix = "clv",
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            Db.GeneratedServices.Add(new GeneratedService
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                VerticalId = verticalId,
                ServiceName = serviceName,
                ServiceType = "RestApi",
                ManifestJson = System.Text.Json.JsonDocument.Parse(System.Text.Json.JsonSerializer.Serialize(new ForgeManifest
                {
                    Product = "clv",
                    VerticalSlug = slug,
                    ServiceName = serviceName,
                    ServiceType = Pervaxis.Forge.Engine.Manifest.ServiceType.RestApi,
                    ComponentPrefix = "clv",
                    CloudProvider = "AWS",
                    GenesisModules = ["Messaging"],
                    CanvasModules = [],
                    Metadata = new ManifestMetadata
                    {
                        Version = "1.0.0",
                        CreatedAtUtc = new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero)
                    }
                })),
                CloudProvider = "AWS",
                GeneratedAt = DateTimeOffset.UtcNow,
                GeneratedBy = "forge-api"
            });

            Db.SaveChanges();
            return Guid.Parse("22222222-2222-2222-2222-222222222222");
        }

        public async ValueTask DisposeAsync()
        {
            await Db.DisposeAsync();
        }
    }
}
