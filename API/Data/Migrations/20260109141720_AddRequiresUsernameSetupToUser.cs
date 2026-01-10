using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SarasBloggAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRequiresUsernameSetupToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "RequiresUsernameSetup",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequiresUsernameSetup",
                table: "AspNetUsers");
        }
    }
}
