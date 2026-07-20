using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddCuoToDepositos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cuo",
                schema: "public",
                table: "depositos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_depositos_Cuo",
                schema: "public",
                table: "depositos",
                column: "Cuo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_depositos_Cuo",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropColumn(
                name: "Cuo",
                schema: "public",
                table: "depositos");
        }
    }
}
