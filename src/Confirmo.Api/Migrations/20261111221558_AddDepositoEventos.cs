using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDepositoEventos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deposito_eventos",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    DepositoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    Evento = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Origen = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SesionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    OcurridoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RegistradoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposito_eventos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deposito_eventos_depositos_DepositoId",
                        column: x => x.DepositoId,
                        principalSchema: "public",
                        principalTable: "depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_deposito_eventos_profiles_UsuarioId",
                        column: x => x.UsuarioId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deposito_eventos_DepositoId_OcurridoEn",
                schema: "public",
                table: "deposito_eventos",
                columns: new[] { "DepositoId", "OcurridoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_deposito_eventos_Evento_OcurridoEn",
                schema: "public",
                table: "deposito_eventos",
                columns: new[] { "Evento", "OcurridoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_deposito_eventos_UsuarioId_OcurridoEn",
                schema: "public",
                table: "deposito_eventos",
                columns: new[] { "UsuarioId", "OcurridoEn" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposito_eventos",
                schema: "public");
        }
    }
}
