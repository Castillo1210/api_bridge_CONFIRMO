using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Confirmo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceIdToProfiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeviceId",
                schema: "public",
                table: "profiles",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceId",
                schema: "public",
                table: "profiles");
        }
    }
}
