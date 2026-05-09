using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SarasBloggAPI.Data;

#nullable disable

namespace SarasBloggAPI.Data.Migrations
{
    [DbContext(typeof(MyDbContext))]
    [Migration("20260510120000_AddBlogTitleMetadata")]
    public partial class AddBlogTitleMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTitleGenerated",
                table: "Blogg",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ShowTitle",
                table: "Blogg",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTitleGenerated",
                table: "Blogg");

            migrationBuilder.DropColumn(
                name: "ShowTitle",
                table: "Blogg");
        }
    }
}
