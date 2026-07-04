using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddTrabajdoresAndFixFks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_depositos_bancos_BancoId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_empresas_EmpresaId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_profiles_VendedorId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_sucursales_SucursalId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropColumn(
                name: "TrabajadorSucursalId",
                schema: "public",
                table: "depositos");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "sucursales",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                schema: "public",
                table: "profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "empresas",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid[]>(
                name: "WarningIds",
                schema: "public",
                table: "depositos",
                type: "uuid[]",
                nullable: true,
                defaultValueSql: "'{}'::uuid[]",
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]",
                oldDefaultValueSql: "'{}'::uuid[]");

            migrationBuilder.AlterColumn<string>(
                name: "ImagenVoucher",
                schema: "public",
                table: "depositos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid[]>(
                name: "ErrorIds",
                schema: "public",
                table: "depositos",
                type: "uuid[]",
                nullable: true,
                defaultValueSql: "'{}'::uuid[]",
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]",
                oldDefaultValueSql: "'{}'::uuid[]");

            migrationBuilder.AddColumn<Guid>(
                name: "trabajador_id",
                schema: "public",
                table: "depositos",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "bancos",
                type: "uuid",
                nullable: false,
                defaultValueSql: "gen_random_uuid()",
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.CreateTable(
                name: "cuentasbancarias",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    NumeroCuenta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Anexo = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    BancoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentasbancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_cuentasbancarias_bancos_BancoId",
                        column: x => x.BancoId,
                        principalSchema: "public",
                        principalTable: "bancos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cuentasbancarias_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "public",
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "trabajadores",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    TelefonoPersonal = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "now()"),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoPor = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_trabajadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_trabajadores_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "public",
                        principalTable: "empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trabajadores_profiles_CreadoPor",
                        column: x => x.CreadoPor,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trabajadores_profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_trabajadores_sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalSchema: "public",
                        principalTable: "sucursales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_sucursales_EmpresaId",
                schema: "public",
                table: "sucursales",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_Email",
                schema: "public",
                table: "profiles",
                column: "Email",
                unique: true,
                filter: "email IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_EmpresaId",
                schema: "public",
                table: "profiles",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_SucursalId",
                schema: "public",
                table: "profiles",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_depositos_trabajador_id",
                schema: "public",
                table: "depositos",
                column: "trabajador_id");

            migrationBuilder.CreateIndex(
                name: "IX_depositos_ValidadoPor",
                schema: "public",
                table: "depositos",
                column: "ValidadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_cuentasbancarias_BancoId",
                schema: "public",
                table: "cuentasbancarias",
                column: "BancoId");

            migrationBuilder.CreateIndex(
                name: "IX_cuentasbancarias_EmpresaId",
                schema: "public",
                table: "cuentasbancarias",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "idx_trabajadores_empresa_sucursal",
                schema: "public",
                table: "trabajadores",
                columns: new[] { "EmpresaId", "SucursalId" });

            migrationBuilder.CreateIndex(
                name: "idx_trabajadores_profile_activo",
                schema: "public",
                table: "trabajadores",
                columns: new[] { "ProfileId", "Activo" });

            migrationBuilder.CreateIndex(
                name: "IX_trabajadores_CreadoPor",
                schema: "public",
                table: "trabajadores",
                column: "CreadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_trabajadores_SucursalId",
                schema: "public",
                table: "trabajadores",
                column: "SucursalId");

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_bancos_BancoId",
                schema: "public",
                table: "depositos",
                column: "BancoId",
                principalSchema: "public",
                principalTable: "bancos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_empresas_EmpresaId",
                schema: "public",
                table: "depositos",
                column: "EmpresaId",
                principalSchema: "public",
                principalTable: "empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_profiles_ValidadoPor",
                schema: "public",
                table: "depositos",
                column: "ValidadoPor",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_profiles_VendedorId",
                schema: "public",
                table: "depositos",
                column: "VendedorId",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_sucursales_SucursalId",
                schema: "public",
                table: "depositos",
                column: "SucursalId",
                principalSchema: "public",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_trabajadores_trabajador_id",
                schema: "public",
                table: "depositos",
                column: "trabajador_id",
                principalSchema: "public",
                principalTable: "trabajadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_profiles_empresas_EmpresaId",
                schema: "public",
                table: "profiles",
                column: "EmpresaId",
                principalSchema: "public",
                principalTable: "empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_profiles_sucursales_SucursalId",
                schema: "public",
                table: "profiles",
                column: "SucursalId",
                principalSchema: "public",
                principalTable: "sucursales",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_sucursales_empresas_EmpresaId",
                schema: "public",
                table: "sucursales",
                column: "EmpresaId",
                principalSchema: "public",
                principalTable: "empresas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_depositos_bancos_BancoId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_empresas_EmpresaId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_profiles_ValidadoPor",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_profiles_VendedorId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_sucursales_SucursalId",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_depositos_trabajadores_trabajador_id",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropForeignKey(
                name: "FK_profiles_empresas_EmpresaId",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_profiles_sucursales_SucursalId",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropForeignKey(
                name: "FK_sucursales_empresas_EmpresaId",
                schema: "public",
                table: "sucursales");

            migrationBuilder.DropTable(
                name: "cuentasbancarias",
                schema: "public");

            migrationBuilder.DropTable(
                name: "trabajadores",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_sucursales_EmpresaId",
                schema: "public",
                table: "sucursales");

            migrationBuilder.DropIndex(
                name: "IX_profiles_Email",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_profiles_EmpresaId",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_profiles_SucursalId",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropIndex(
                name: "IX_depositos_trabajador_id",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropIndex(
                name: "IX_depositos_ValidadoPor",
                schema: "public",
                table: "depositos");

            migrationBuilder.DropColumn(
                name: "Email",
                schema: "public",
                table: "profiles");

            migrationBuilder.DropColumn(
                name: "trabajador_id",
                schema: "public",
                table: "depositos");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "sucursales",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "public",
                table: "profiles",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "empresas",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AlterColumn<Guid[]>(
                name: "WarningIds",
                schema: "public",
                table: "depositos",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]",
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]",
                oldNullable: true,
                oldDefaultValueSql: "'{}'::uuid[]");

            migrationBuilder.AlterColumn<string>(
                name: "ImagenVoucher",
                schema: "public",
                table: "depositos",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid[]>(
                name: "ErrorIds",
                schema: "public",
                table: "depositos",
                type: "uuid[]",
                nullable: false,
                defaultValueSql: "'{}'::uuid[]",
                oldClrType: typeof(Guid[]),
                oldType: "uuid[]",
                oldNullable: true,
                oldDefaultValueSql: "'{}'::uuid[]");

            migrationBuilder.AddColumn<long>(
                name: "TrabajadorSucursalId",
                schema: "public",
                table: "depositos",
                type: "bigint",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                schema: "public",
                table: "bancos",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldDefaultValueSql: "gen_random_uuid()");

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_bancos_BancoId",
                schema: "public",
                table: "depositos",
                column: "BancoId",
                principalSchema: "public",
                principalTable: "bancos",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_empresas_EmpresaId",
                schema: "public",
                table: "depositos",
                column: "EmpresaId",
                principalSchema: "public",
                principalTable: "empresas",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_profiles_VendedorId",
                schema: "public",
                table: "depositos",
                column: "VendedorId",
                principalSchema: "public",
                principalTable: "profiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_depositos_sucursales_SucursalId",
                schema: "public",
                table: "depositos",
                column: "SucursalId",
                principalSchema: "public",
                principalTable: "sucursales",
                principalColumn: "Id");
        }
    }
}
