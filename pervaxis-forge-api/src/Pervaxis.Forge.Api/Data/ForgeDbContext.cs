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

using Microsoft.EntityFrameworkCore;
using Pervaxis.Forge.Api.Data.Entities;
using System.Text.Json;

namespace Pervaxis.Forge.Api.Data;

public class ForgeDbContext(
    DbContextOptions<ForgeDbContext> options) : DbContext(options)
{

    public DbSet<Vertical> Verticals => Set<Vertical>();
    public DbSet<VerticalCloudConfig> VerticalCloudConfigs => Set<VerticalCloudConfig>();
    public DbSet<VerticalSourceControlConfig> VerticalSourceControlConfigs => Set<VerticalSourceControlConfig>();
    public DbSet<VerticalTechDefaults> VerticalTechDefaults => Set<VerticalTechDefaults>();
    public DbSet<GenerationLog> GenerationLogs => Set<GenerationLog>();
    public DbSet<GeneratedService> GeneratedServices => Set<GeneratedService>();
    public DbSet<DeploymentOutput> DeploymentOutputs => Set<DeploymentOutput>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── Vertical ─────────────────────────────────────────────────────────
        modelBuilder.Entity<Vertical>(e =>
        {
            e.ToTable("verticals");
            e.HasKey(v => v.Id);
            e.Property(v => v.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(v => v.Slug).HasMaxLength(100).IsRequired();
            e.Property(v => v.DisplayName).HasMaxLength(255).IsRequired();
            e.Property(v => v.OwnerTeam).HasMaxLength(255).IsRequired();
            e.Property(v => v.OwnerEmail).HasMaxLength(255).IsRequired();
            e.Property(v => v.ComponentPrefix).HasMaxLength(5).IsRequired();
            e.Property(v => v.IsActive).HasDefaultValue(true);
            e.Property(v => v.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(v => v.UpdatedAt).HasDefaultValueSql("NOW()");

            e.HasIndex(v => v.Slug).IsUnique().HasDatabaseName("idx_verticals_slug");

            e.HasOne(v => v.CloudConfig)
                .WithOne(c => c.Vertical)
                .HasForeignKey<VerticalCloudConfig>(c => c.VerticalId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(v => v.SourceControlConfig)
                .WithOne(c => c.Vertical)
                .HasForeignKey<VerticalSourceControlConfig>(c => c.VerticalId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(v => v.TechDefaults)
                .WithOne(t => t.Vertical)
                .HasForeignKey<VerticalTechDefaults>(t => t.VerticalId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(v => v.GenerationLogs)
                .WithOne(g => g.Vertical)
                .HasForeignKey(g => g.VerticalId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(v => v.GeneratedServices)
                .WithOne(s => s.Vertical)
                .HasForeignKey(s => s.VerticalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── VerticalCloudConfig ───────────────────────────────────────────────
        modelBuilder.Entity<VerticalCloudConfig>(e =>
        {
            e.ToTable("vertical_cloud_configs");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(c => c.Provider).HasMaxLength(50).HasDefaultValue("AWS").IsRequired();
            e.Property(c => c.AwsAccountId).HasMaxLength(12);
            e.Property(c => c.DefaultRegion).HasMaxLength(50).HasDefaultValue("us-east-1").IsRequired();
            e.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");

            e.HasIndex(c => new { c.VerticalId, c.Provider }).IsUnique();

            e.Property(c => c.IamRoleArn);
        });

        // ── VerticalSourceControlConfig ───────────────────────────────────────
        modelBuilder.Entity<VerticalSourceControlConfig>(e =>
        {
            e.ToTable("vertical_source_control_configs");
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(c => c.Platform).HasMaxLength(50).HasDefaultValue("GitHub").IsRequired();
            e.Property(c => c.GitHubOrg).HasMaxLength(255);
            e.Property(c => c.DefaultVisibility).HasMaxLength(20).HasDefaultValue("Private").IsRequired();
            e.Property(c => c.DefaultBranchProtection).HasDefaultValue(true);
            e.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");

            e.HasIndex(c => new { c.VerticalId, c.Platform }).IsUnique();

            e.Property(c => c.AccessToken);
        });

        // ── VerticalTechDefaults ──────────────────────────────────────────────
        modelBuilder.Entity<VerticalTechDefaults>(e =>
        {
            e.ToTable("vertical_tech_defaults");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(t => t.Environments).HasColumnType("text[]").IsRequired();
            e.Property(t => t.DefaultEnvironment).HasMaxLength(50).HasDefaultValue("test").IsRequired();
            e.Property(t => t.GenerateTerraform).HasDefaultValue(true);
            e.Property(t => t.GenerateCdk).HasDefaultValue(true);
            e.Property(t => t.DefaultDbEngine).HasMaxLength(50);
            e.Property(t => t.CreatedAt).HasDefaultValueSql("NOW()");
            e.Property(t => t.UpdatedAt).HasDefaultValueSql("NOW()");

            e.HasIndex(t => t.VerticalId).IsUnique();
        });

        // ── GenerationLog ─────────────────────────────────────────────────────
        modelBuilder.Entity<GenerationLog>(e =>
        {
            e.ToTable("generation_logs");
            e.HasKey(g => g.Id);
            e.Property(g => g.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(g => g.Manifest)
                .HasConversion(
                    v => v.RootElement.GetRawText(),
                    v => JsonDocument.Parse(v))
                .HasColumnType("jsonb")
                .IsRequired();
            e.Property(g => g.InfrastructureDeployed).HasDefaultValue(false);
            e.Property(g => g.GitHubReposCreated).HasDefaultValue(false);
            e.Property(g => g.CreatedBy).HasMaxLength(255).IsRequired();
            e.Property(g => g.CreatedAt).HasDefaultValueSql("NOW()");

            e.HasIndex(g => g.VerticalId).HasDatabaseName("idx_generation_logs_vertical");
            e.HasIndex(g => g.CreatedAt).IsDescending().HasDatabaseName("idx_generation_logs_created_at");

            e.HasMany(g => g.DeploymentOutputs)
                .WithOne(d => d.GenerationLog)
                .HasForeignKey(d => d.GenerationLogId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ── DeploymentOutput ──────────────────────────────────────────────────
        modelBuilder.Entity<DeploymentOutput>(e =>
        {
            e.ToTable("deployment_outputs");
            e.HasKey(d => d.Id);
            e.Property(d => d.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(d => d.ServiceName).HasMaxLength(255).IsRequired();
            e.Property(d => d.ResourceType).HasMaxLength(100).IsRequired();
            e.Property(d => d.ResourceName).HasMaxLength(500).IsRequired();
            e.Property(d => d.CreatedAt).HasDefaultValueSql("NOW()");

            e.HasIndex(d => d.GenerationLogId).HasDatabaseName("idx_deployment_outputs_generation");
        });

        // ── GeneratedService ─────────────────────────────────────────────────
        modelBuilder.Entity<GeneratedService>(e =>
        {
            e.ToTable("generated_services");
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasDefaultValueSql("gen_random_uuid()");
            e.Property(s => s.ServiceName).HasMaxLength(255).IsRequired();
            e.Property(s => s.ServiceType).HasMaxLength(50).IsRequired();
            e.Property(s => s.ManifestJson)
                .HasConversion(
                    v => v.RootElement.GetRawText(),
                    v => JsonDocument.Parse(v))
                .HasColumnType("jsonb")
                .IsRequired();
            e.Property(s => s.CloudProvider).HasMaxLength(50).IsRequired();
            e.Property(s => s.GeneratedAt).HasDefaultValueSql("NOW()");
            e.Property(s => s.GeneratedBy).HasMaxLength(255).IsRequired();

            e.HasIndex(s => new { s.VerticalId, s.ServiceName }).IsUnique().HasDatabaseName("idx_generated_services_vertical_name");
            e.HasIndex(s => new { s.VerticalId, s.GeneratedAt }).HasDatabaseName("idx_generated_services_vertical_generated_at");
        });
    }
}
