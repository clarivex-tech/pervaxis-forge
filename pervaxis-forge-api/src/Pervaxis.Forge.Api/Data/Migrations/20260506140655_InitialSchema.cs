using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pervaxis.Forge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "verticals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Slug = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OwnerTeam = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    OwnerEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_verticals", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "generation_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    VerticalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Manifest = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    ServiceCount = table.Column<int>(type: "integer", nullable: false),
                    InfrastructureDeployed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    GitHubReposCreated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_generation_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_generation_logs_verticals_VerticalId",
                        column: x => x.VerticalId,
                        principalTable: "verticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "vertical_cloud_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    VerticalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Provider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "AWS"),
                    AwsAccountId = table.Column<string>(type: "character varying(12)", maxLength: 12, nullable: true),
                    IamRoleArn = table.Column<string>(type: "text", nullable: true),
                    DefaultRegion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "us-east-1"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vertical_cloud_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vertical_cloud_configs_verticals_VerticalId",
                        column: x => x.VerticalId,
                        principalTable: "verticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vertical_source_control_configs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    VerticalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "GitHub"),
                    GitHubOrg = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AccessToken = table.Column<string>(type: "text", nullable: true),
                    DefaultVisibility = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Private"),
                    DefaultBranchProtection = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vertical_source_control_configs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vertical_source_control_configs_verticals_VerticalId",
                        column: x => x.VerticalId,
                        principalTable: "verticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "vertical_tech_defaults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    VerticalId = table.Column<Guid>(type: "uuid", nullable: false),
                    Environments = table.Column<string[]>(type: "text[]", nullable: false),
                    DefaultEnvironment = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "test"),
                    GenerateTerraform = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GenerateCdk = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    DefaultDbEngine = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_vertical_tech_defaults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_vertical_tech_defaults_verticals_VerticalId",
                        column: x => x.VerticalId,
                        principalTable: "verticals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "deployment_outputs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    GenerationLogId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ResourceType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ResourceName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    EndpointOrArn = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deployment_outputs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deployment_outputs_generation_logs_GenerationLogId",
                        column: x => x.GenerationLogId,
                        principalTable: "generation_logs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_deployment_outputs_generation",
                table: "deployment_outputs",
                column: "GenerationLogId");

            migrationBuilder.CreateIndex(
                name: "idx_generation_logs_created_at",
                table: "generation_logs",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "idx_generation_logs_vertical",
                table: "generation_logs",
                column: "VerticalId");

            migrationBuilder.CreateIndex(
                name: "IX_vertical_cloud_configs_VerticalId",
                table: "vertical_cloud_configs",
                column: "VerticalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vertical_cloud_configs_VerticalId_Provider",
                table: "vertical_cloud_configs",
                columns: new[] { "VerticalId", "Provider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vertical_source_control_configs_VerticalId",
                table: "vertical_source_control_configs",
                column: "VerticalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vertical_source_control_configs_VerticalId_Platform",
                table: "vertical_source_control_configs",
                columns: new[] { "VerticalId", "Platform" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_vertical_tech_defaults_VerticalId",
                table: "vertical_tech_defaults",
                column: "VerticalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_verticals_slug",
                table: "verticals",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deployment_outputs");

            migrationBuilder.DropTable(
                name: "vertical_cloud_configs");

            migrationBuilder.DropTable(
                name: "vertical_source_control_configs");

            migrationBuilder.DropTable(
                name: "vertical_tech_defaults");

            migrationBuilder.DropTable(
                name: "generation_logs");

            migrationBuilder.DropTable(
                name: "verticals");
        }
    }
}
