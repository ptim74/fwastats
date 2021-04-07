using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class TownHall12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TH12Count",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Th12Count",
                table: "Clans",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TH12Count",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Th12Count",
                table: "Clans");
        }
    }
}
