using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class AddLeagueAndLocation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LeagueName",
                table: "Members",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LocationName",
                table: "Clans",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WarFrequency",
                table: "Clans",
                maxLength: 20,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LeagueName",
                table: "Members");

            migrationBuilder.DropColumn(
                name: "LocationName",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "WarFrequency",
                table: "Clans");
        }
    }
}
