using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class TownHall13 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TH13Count",
                table: "WeightResults",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Th13Count",
                table: "Clans",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TH13Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Th13Count",
                table: "Clans");
        }
    }
}
