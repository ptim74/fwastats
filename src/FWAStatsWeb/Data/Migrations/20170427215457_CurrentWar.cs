using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class CurrentWar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WarAttacks",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        //.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AttackerTag = table.Column<string>(maxLength: 15, nullable: true),
                    DefenderMapPosition = table.Column<int>(nullable: false),
                    DefenderTag = table.Column<string>(maxLength: 15, nullable: true),
                    DefenderTownHallLevel = table.Column<int>(nullable: false),
                    DestructionPercentage = table.Column<int>(nullable: false),
                    Order = table.Column<int>(nullable: false),
                    Stars = table.Column<int>(nullable: false),
                    WarID = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarAttacks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WarAttacks_Wars_WarID",
                        column: x => x.WarID,
                        principalTable: "Wars",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarMembers",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        //.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    MapPosition = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    OpponentAttacks = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(maxLength: 15, nullable: true),
                    TownHallLevel = table.Column<int>(nullable: false),
                    WarID = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarMembers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_WarMembers_Wars_WarID",
                        column: x => x.WarID,
                        principalTable: "Wars",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WarAttacks_AttackerTag",
                table: "WarAttacks",
                column: "AttackerTag");

            migrationBuilder.CreateIndex(
                name: "IX_WarAttacks_DefenderTag",
                table: "WarAttacks",
                column: "DefenderTag");

            migrationBuilder.CreateIndex(
                name: "IX_WarAttacks_WarID_Order",
                table: "WarAttacks",
                columns: new[] { "WarID", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_WarMembers_Tag",
                table: "WarMembers",
                column: "Tag");

            migrationBuilder.CreateIndex(
                name: "IX_WarMembers_WarID_MapPosition",
                table: "WarMembers",
                columns: new[] { "WarID", "MapPosition" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WarAttacks");

            migrationBuilder.DropTable(
                name: "WarMembers");
        }
    }
}
