using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWAStatsWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class TownHall19 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Th19Count",
                table: "Clans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Th19Count",
                table: "Clans");
        }
    }
}
