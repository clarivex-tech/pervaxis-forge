using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pervaxis.Forge.Api.Data.Migrations;

public partial class AddGeneratedServices : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "generated_services",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                VerticalId = table.Column<Guid>(type: "uuid", nullable: false),
                ServiceName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                ServiceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                ManifestJson = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                CloudProvider = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                GeneratedBy = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_generated_services", x => x.Id);
                table.ForeignKey(
                    name: "FK_generated_services_verticals_VerticalId",
                    column: x => x.VerticalId,
                    principalTable: "verticals",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "idx_generated_services_vertical_generated_at",
            table: "generated_services",
            columns: new[] { "VerticalId", "GeneratedAt" });

        migrationBuilder.CreateIndex(
            name: "idx_generated_services_vertical_name",
            table: "generated_services",
            columns: new[] { "VerticalId", "ServiceName" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "generated_services");
    }
}
