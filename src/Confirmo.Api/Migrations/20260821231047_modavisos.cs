using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class modavisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ZavuPlantillaCodigo",
                schema: "public",
                table: "avisos",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "zavu_plantillas",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TemplateId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreadoPor = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_zavu_plantillas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_zavu_plantillas_profiles_CreadoPor",
                        column: x => x.CreadoPor,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_zavu_plantillas_Codigo",
                schema: "public",
                table: "zavu_plantillas",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_zavu_plantillas_CreadoPor",
                schema: "public",
                table: "zavu_plantillas",
                column: "CreadoPor");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "zavu_plantillas",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "ZavuPlantillaCodigo",
                schema: "public",
                table: "avisos");
        }
    }
}
