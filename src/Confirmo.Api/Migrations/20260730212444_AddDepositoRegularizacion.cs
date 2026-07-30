using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositoRegularizacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deposito_regularizaciones",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepositoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Accion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Motivo = table.Column<string>(type: "text", nullable: true),
                    ImagenAnterior = table.Column<string>(type: "text", nullable: true),
                    ImagenNueva = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposito_regularizaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deposito_regularizaciones_depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalSchema: "public",
                        principalTable: "depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deposito_regularizaciones_profiles_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deposito_regularizaciones_CreatedAt",
                schema: "public",
                table: "deposito_regularizaciones",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_deposito_regularizaciones_DepositoId_CreatedAt",
                schema: "public",
                table: "deposito_regularizaciones",
                columns: new[] { "DepositoId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_deposito_regularizaciones_UsuarioId",
                schema: "public",
                table: "deposito_regularizaciones",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposito_regularizaciones",
                schema: "public");
        }
    }
}
