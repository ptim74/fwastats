using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class AddClanGroup : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Matched",
                table: "Wars",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Synced",
                table: "Wars",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ClanGroup",
                table: "UpdateTasks",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "ClanValidities",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Group",
                table: "Clans",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Matched",
                table: "Wars");

            migrationBuilder.DropColumn(
                name: "Synced",
                table: "Wars");

            migrationBuilder.DropColumn(
                name: "ClanGroup",
                table: "UpdateTasks");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "ClanValidities");

            migrationBuilder.DropColumn(
                name: "Group",
                table: "Clans");
        }
    }
}
