using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class AddClashIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_WarSyncs_Finish",
                table: "WarSyncs",
                column: "Finish");

            migrationBuilder.CreateIndex(
                name: "IX_WarSyncs_Start",
                table: "WarSyncs",
                column: "Start");

            migrationBuilder.CreateIndex(
                name: "IX_WarOpponents_Tag",
                table: "WarOpponents",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_WarParticipants_Tag",
                table: "WarParticipants",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_EndTime",
                table: "Wars",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Name",
                table: "Members",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Role",
                table: "Members",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Clans_Name",
                table: "Clans",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WarSyncs_Finish",
                table: "WarSyncs");

            migrationBuilder.DropIndex(
                name: "IX_WarSyncs_Start",
                table: "WarSyncs");

            migrationBuilder.DropIndex(
                name: "IX_WarOpponents_Tag",
                table: "WarOpponents");

            migrationBuilder.DropIndex(
                name: "IX_WarParticipants_Tag",
                table: "WarParticipants");

            migrationBuilder.DropIndex(
                name: "IX_Wars_EndTime",
                table: "Wars");

            migrationBuilder.DropIndex(
                name: "IX_Members_Name",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Members_Role",
                table: "Members");

            migrationBuilder.DropIndex(
                name: "IX_Clans_Name",
                table: "Clans");
        }
    }
}
