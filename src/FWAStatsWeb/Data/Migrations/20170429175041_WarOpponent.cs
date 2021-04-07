using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class WarOpponent : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOpponent",
                table: "WarMembers",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpponent",
                table: "WarAttacks",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOpponent",
                table: "WarMembers");

            migrationBuilder.DropColumn(
                name: "IsOpponent",
                table: "WarAttacks");
        }
    }
}
