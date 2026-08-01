using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddAvisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "avisos",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Titulo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MensajeTexto = table.Column<string>(type: "text", nullable: false),
                    MediaUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    TipoMedia = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RolesDestino = table.Column<string[]>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
                    EnviarApp = table.Column<bool>(type: "boolean", nullable: false),
                    EnviarWhatsapp = table.Column<bool>(type: "boolean", nullable: false),
                    EnviarEmail = table.Column<bool>(type: "boolean", nullable: false),
                    AsuntoEmail = table.Column<string>(type: "character varying(299)", maxLength: 299, nullable: true),
                    EsRecurrente = table.Column<bool>(type: "boolean", nullable: false),
                    Frecuencia = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: true),
                    HoraEjecucion = table.Column<TimeSpan>(type: "interval", nullable: true),
                    DiaSemana = table.Column<int>(type: "integer", nullable: true),
                    DiaMes = table.Column<int>(type: "integer", nullable: true),
                    ProximaEjecucion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UltimaEjecucion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreadoPor = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    Estado = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: false, defaultValue: "programado"),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_avisos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_avisos_profiles_CreadoPor",
                        column: x => x.CreadoPor,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "envio_aviso_logs",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    AvisoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Canal = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: false),
                    Estado = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: false),
                    ZavuMessageId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envio_aviso_logs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_envio_aviso_logs_avisos_AvisoId",
                        column: x => x.AvisoId,
                        principalSchema: "public",
                        principalTable: "avisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_envio_aviso_logs_profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_avisos_estado_proxima_ejecucion",
                schema: "public",
                table: "avisos",
                columns: new[] { "Estado", "ProximaEjecucion" });

            migrationBuilder.CreateIndex(
                name: "idx_avisos_roles_destino",
                schema: "public",
                table: "avisos",
                column: "RolesDestino")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_avisos_CreadoPor",
                schema: "public",
                table: "avisos",
                column: "CreadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_envio_aviso_logs_AvisoId_ProfileId_Canal",
                schema: "public",
                table: "envio_aviso_logs",
                columns: new[] { "AvisoId", "ProfileId", "Canal" });

            migrationBuilder.CreateIndex(
                name: "IX_envio_aviso_logs_ProfileId",
                schema: "public",
                table: "envio_aviso_logs",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "envio_aviso_logs",
                schema: "public");

            migrationBuilder.DropTable(
                name: "avisos",
                schema: "public");
        }
    }
}
