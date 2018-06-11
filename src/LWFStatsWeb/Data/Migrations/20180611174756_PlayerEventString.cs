using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class PlayerEventString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "StringValue",
                table: "PlayerEvents",
                type: "TEXT",
                maxLength: 10,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StringValue",
                table: "PlayerEvents");
        }
    }
}
