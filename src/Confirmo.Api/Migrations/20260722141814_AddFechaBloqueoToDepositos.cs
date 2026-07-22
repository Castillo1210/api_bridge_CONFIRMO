using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaBloqueoToDepositos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FechaBloqueo",
                schema: "public",
                table: "depositos",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaBloqueo",
                schema: "public",
                table: "depositos");
        }
    }
}
