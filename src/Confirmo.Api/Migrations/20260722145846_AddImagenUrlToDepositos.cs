using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddImagenUrlToDepositos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagenUrl",
                schema: "public",
                table: "depositos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagenUrl",
                schema: "public",
                table: "depositos");
        }
    }
}
