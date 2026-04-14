using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWAStatsWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWeightColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Base01",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base02",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base03",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base04",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base05",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base06",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base07",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base08",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base09",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base10",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base11",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base12",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base13",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base14",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base15",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base16",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base17",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base18",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base19",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base20",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base21",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base22",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base23",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base24",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base25",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base26",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base27",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base28",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base29",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base30",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base31",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base32",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base33",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base34",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base35",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base36",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base37",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base38",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base39",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base40",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base41",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base42",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base43",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base44",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base45",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base46",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base47",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base48",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base49",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base50",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "PendingResult",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH10Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH11Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH12Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH13Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH14Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH15Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH16Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH17Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH18Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH7Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH8Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "TH9Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "THSum",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Weight",
                table: "WeightResults");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Base01",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base02",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base03",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base04",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base05",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base06",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base07",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base08",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base09",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base10",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base11",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base12",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base13",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base14",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base15",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base16",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base17",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base18",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base19",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base20",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base21",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base22",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base23",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base24",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base25",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base26",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base27",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base28",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base29",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base30",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base31",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base32",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base33",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base34",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base35",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base36",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base37",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base38",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base39",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base40",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base41",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base42",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base43",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base44",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base45",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base46",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base47",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base48",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base49",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base50",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PendingResult",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TH10Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH11Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH12Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH13Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH14Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH15Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH16Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH17Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH18Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH7Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH8Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TH9Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "THSum",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Weight",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }
    }
}
