using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class ClanValidity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClanValidities",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 10, nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    ValidFrom = table.Column<DateTime>(nullable: false),
                    ValidTo = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanValidities", x => x.Tag);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClanValidities_ValidFrom",
                table: "ClanValidities",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ClanValidities_ValidTo",
                table: "ClanValidities",
                column: "ValidTo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanValidities");
        }
    }
}
