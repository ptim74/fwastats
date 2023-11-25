using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWAStatsWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class TownHall16 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TH16Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Th16Count",
                table: "Clans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TH16Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Th16Count",
                table: "Clans");
        }
    }
}
