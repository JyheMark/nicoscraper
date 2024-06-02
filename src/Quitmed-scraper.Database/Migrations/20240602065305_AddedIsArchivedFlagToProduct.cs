using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Quitmed_scraper.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsArchivedFlagToProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "Products",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "Products");
        }
    }
}
