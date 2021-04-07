using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class PreparationStartTimeIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Wars_PreparationStartTime",
                table: "Wars",
                column: "PreparationStartTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Wars_PreparationStartTime",
                table: "Wars");
        }
    }
}
