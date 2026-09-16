using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositoRechazoHistorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deposito_rechazos_historial",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepositoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ImagenVoucherRechazada = table.Column<string>(type: "text", nullable: true),
                    MotivoRechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Observaciones = table.Column<string>(type: "text", nullable: true),
                    FechaRechazo = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RechazadoPor = table.Column<Guid>(type: "uuid", nullable: true),
                    RegularizadoPor = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposito_rechazos_historial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deposito_rechazos_historial_depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalSchema: "public",
                        principalTable: "depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deposito_rechazos_historial_profiles_RechazadoPor",
                        column: x => x.RechazadoPor,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_deposito_rechazos_historial_profiles_RegularizadoPor",
                        column: x => x.RegularizadoPor,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deposito_rechazos_historial_DepositoId_CreatedAt",
                schema: "public",
                table: "deposito_rechazos_historial",
                columns: new[] { "DepositoId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_deposito_rechazos_historial_RechazadoPor",
                schema: "public",
                table: "deposito_rechazos_historial",
                column: "RechazadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_deposito_rechazos_historial_RegularizadoPor",
                schema: "public",
                table: "deposito_rechazos_historial",
                column: "RegularizadoPor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposito_rechazos_historial",
                schema: "public");
        }
    }
}
