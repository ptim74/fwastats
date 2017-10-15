using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class Players : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 10, nullable: false),
                    AttackWins = table.Column<int>(nullable: false),
                    BestTrophies = table.Column<int>(nullable: false),
                    DefenseWins = table.Column<int>(nullable: false),
                    LastUpdated = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    TownHallLevel = table.Column<int>(nullable: false),
                    WarStars = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Tag);
                });

            migrationBuilder.CreateTable(
                name: "PlayerEvents",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        //.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ClanTag = table.Column<string>(maxLength: 10, nullable: true),
                    EventDate = table.Column<DateTime>(nullable: false),
                    EventType = table.Column<int>(nullable: false),
                    PlayerTag = table.Column<string>(maxLength: 10, nullable: true),
                    Value = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerEvents", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEvents_ClanTag_EventDate",
                table: "PlayerEvents",
                columns: new[] { "ClanTag", "EventDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerEvents_PlayerTag_EventDate",
                table: "PlayerEvents",
                columns: new[] { "PlayerTag", "EventDate" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "PlayerEvents");
        }
    }
}
