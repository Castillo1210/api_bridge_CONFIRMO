using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "bancos",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Codigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bancos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "empresas",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ruc = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Logo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "profiles",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: true),
                    Rol = table.Column<string>(type: "character varying(55)", maxLength: 55, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FcmToken = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sucursales",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursales", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "voucher_business_errors",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    ErrorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Severity = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    UserAction = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_voucher_business_errors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "depositos",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    NumeroOperacion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Cliente = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Monto = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    Moneda = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    FechaRegistro = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    ImagenVoucher = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Anexo = table.Column<string>(type: "text", nullable: true),
                    NumeroOperacionBanco = table.Column<string>(type: "text", nullable: true),
                    FechaDeposito = table.Column<DateOnly>(type: "date", nullable: true),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "recibido"),
                    Observaciones = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    MotivoRechazo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaValidacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    EmpresaId = table.Column<Guid>(type: "uuid", nullable: true),
                    BancoId = table.Column<Guid>(type: "uuid", nullable: true),
                    SucursalId = table.Column<Guid>(type: "uuid", nullable: true),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValidadoPor = table.Column<Guid>(type: "uuid", nullable: true),
                    TrabajadorSucursalId = table.Column<long>(type: "bigint", nullable: true),
                    ReferenciaCliente = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DatosOcr = table.Column<object>(type: "jsonb", nullable: true),
                    TelefonoOrigen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Condicion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Riesgo = table.Column<bool>(type: "boolean", nullable: false),
                    RucCliente = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    EsAntiguo = table.Column<bool>(type: "boolean", nullable: true),
                    FechaSoloDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ErrorIds = table.Column<Guid[]>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'::uuid[]"),
                    WarningIds = table.Column<Guid[]>(type: "uuid[]", nullable: false, defaultValueSql: "'{}'::uuid[]")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_depositos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_depositos_bancos_BancoId",
                        column: x => x.BancoId,
                        principalSchema: "public",
                        principalTable: "bancos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_depositos_empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalSchema: "public",
                        principalTable: "empresas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_depositos_profiles_VendedorId",
                        column: x => x.VendedorId,
                        principalSchema: "public",
                        principalTable: "profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_depositos_sucursales_SucursalId",
                        column: x => x.SucursalId,
                        principalSchema: "public",
                        principalTable: "sucursales",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "deposit_messages",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    DepositId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    MessageType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Metadata = table.Column<object>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deposit_messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_deposit_messages_depositos_DepositId",
                        column: x => x.DepositId,
                        principalSchema: "public",
                        principalTable: "depositos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "idx_deposit_messages_deposit_created",
                schema: "public",
                table: "deposit_messages",
                columns: new[] { "DepositId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "idx_depositos_empresa_estado",
                schema: "public",
                table: "depositos",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "idx_depositos_error_ids",
                schema: "public",
                table: "depositos",
                column: "ErrorIds")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "idx_depositos_estado_fecha",
                schema: "public",
                table: "depositos",
                columns: new[] { "Estado", "FechaRegistro" });

            migrationBuilder.CreateIndex(
                name: "idx_depositos_vendedor_fecha",
                schema: "public",
                table: "depositos",
                columns: new[] { "VendedorId", "FechaRegistro" });

            migrationBuilder.CreateIndex(
                name: "idx_depositos_warning_ids",
                schema: "public",
                table: "depositos",
                column: "WarningIds")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_depositos_BancoId",
                schema: "public",
                table: "depositos",
                column: "BancoId");

            migrationBuilder.CreateIndex(
                name: "IX_depositos_SucursalId",
                schema: "public",
                table: "depositos",
                column: "SucursalId");

            migrationBuilder.CreateIndex(
                name: "IX_profiles_PhoneNumber",
                schema: "public",
                table: "profiles",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_voucher_business_errors_active_severity",
                schema: "public",
                table: "voucher_business_errors",
                columns: new[] { "IsActive", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_voucher_business_errors_ErrorCode",
                schema: "public",
                table: "voucher_business_errors",
                column: "ErrorCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deposit_messages",
                schema: "public");

            migrationBuilder.DropTable(
                name: "voucher_business_errors",
                schema: "public");

            migrationBuilder.DropTable(
                name: "depositos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "bancos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "empresas",
                schema: "public");

            migrationBuilder.DropTable(
                name: "profiles",
                schema: "public");

            migrationBuilder.DropTable(
                name: "sucursales",
                schema: "public");
        }
    }
}
