using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRegularizacionFinanzasToDepositos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PendienteRegularizar",
                schema: "public",
                table: "depositos",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendienteRegularizar",
                schema: "public",
                table: "depositos");
        }
    }
}
