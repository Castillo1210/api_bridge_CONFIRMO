using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpRegularizaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_deposito_regularizaciones_DepositoId",
                schema: "public",
                table: "deposito_regularizaciones",
                column: "DepositoId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_deposito_regularizaciones_DepositoId",
                schema: "public",
                table: "deposito_regularizaciones");
        }
    }
}
