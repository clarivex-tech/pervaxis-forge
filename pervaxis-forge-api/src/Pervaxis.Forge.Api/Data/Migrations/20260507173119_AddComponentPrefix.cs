using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pervaxis.Forge.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddComponentPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ComponentPrefix",
                table: "verticals",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ComponentPrefix",
                table: "verticals");
        }
    }
}
