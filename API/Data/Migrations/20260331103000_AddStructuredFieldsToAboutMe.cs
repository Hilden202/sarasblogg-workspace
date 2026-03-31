using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SarasBloggAPI.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStructuredFieldsToAboutMe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Age",
                table: "AboutMe",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "AboutMe",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Family",
                table: "AboutMe",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AboutMe",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Age",
                table: "AboutMe");

            migrationBuilder.DropColumn(
                name: "City",
                table: "AboutMe");

            migrationBuilder.DropColumn(
                name: "Family",
                table: "AboutMe");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "AboutMe");
        }
    }
}
