using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class UpdateWarOpponentIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wars_OpponentTag",
                table: "Wars");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_OpponentTag_EndTime",
                table: "Wars",
                columns: new[] { "OpponentTag", "EndTime" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wars_OpponentTag_EndTime",
                table: "Wars");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_OpponentTag",
                table: "Wars",
                column: "OpponentTag");
        }
    }
}
