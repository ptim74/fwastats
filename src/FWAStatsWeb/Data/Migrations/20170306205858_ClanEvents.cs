using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class ClanEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClanEvents",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        //.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Activity = table.Column<int>(nullable: false),
                    ClanTag = table.Column<string>(maxLength: 15, nullable: true),
                    Donations = table.Column<int>(nullable: false),
                    EventDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanEvents", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClanEvents_EventDate",
                table: "ClanEvents",
                column: "EventDate");

            migrationBuilder.CreateIndex(
                name: "IX_ClanEvents_ClanTag_EventDate",
                table: "ClanEvents",
                columns: new[] { "ClanTag", "EventDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanEvents");
        }
    }
}
