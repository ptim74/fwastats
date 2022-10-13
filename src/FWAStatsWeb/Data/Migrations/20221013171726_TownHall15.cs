using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWAStatsWeb.Data.Migrations
{
    public partial class TownHall15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TH15Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Th15Count",
                table: "Clans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TH15Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Th15Count",
                table: "Clans");
        }
    }
}
