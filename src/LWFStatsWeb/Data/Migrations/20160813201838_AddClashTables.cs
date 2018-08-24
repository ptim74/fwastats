using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class AddClashTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clans",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 15, nullable: false),
                    BadgeUrl = table.Column<string>(maxLength: 150, nullable: true),
                    ClanLevel = table.Column<int>(nullable: false),
                    ClanPoints = table.Column<int>(nullable: false),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    Group = table.Column<string>(maxLength: 10, nullable: true),
                    IsWarLogPublic = table.Column<bool>(nullable: false),
                    LocationName = table.Column<string>(maxLength: 30, nullable: true),
                    Members = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    RequiredTrophies = table.Column<int>(nullable: false),
                    Type = table.Column<string>(maxLength: 15, nullable: true),
                    WarFrequency = table.Column<string>(maxLength: 20, nullable: true),
                    WarLosses = table.Column<int>(nullable: false),
                    WarTies = table.Column<int>(nullable: false),
                    WarWinStreak = table.Column<int>(nullable: false),
                    WarWins = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clans", x => x.Tag);
                });

            migrationBuilder.CreateTable(
                name: "ClanValidities",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 15, nullable: false),
                    Group = table.Column<string>(maxLength: 10, nullable: true),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    ValidFrom = table.Column<DateTime>(nullable: false),
                    ValidTo = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanValidities", x => x.Tag);
                });

            migrationBuilder.CreateTable(
                name: "UpdateTasks",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ClanGroup = table.Column<string>(maxLength: 10, nullable: true),
                    ClanName = table.Column<string>(maxLength: 50, nullable: true),
                    ClanTag = table.Column<string>(maxLength: 15, nullable: true),
                    Mode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateTasks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Wars",
                columns: table => new
                {
                    ID = table.Column<string>(maxLength: 30, nullable: false),
                    ClanAttacks = table.Column<int>(nullable: false),
                    ClanBadgeUrl = table.Column<string>(maxLength: 150, nullable: true),
                    ClanDestructionPercentage = table.Column<double>(nullable: false),
                    ClanExpEarned = table.Column<int>(nullable: false),
                    ClanLevel = table.Column<int>(nullable: false),
                    ClanName = table.Column<string>(maxLength: 50, nullable: true),
                    ClanStars = table.Column<int>(nullable: false),
                    ClanTag = table.Column<string>(maxLength: 15, nullable: true),
                    EndTime = table.Column<DateTime>(nullable: false),
                    Matched = table.Column<bool>(nullable: false),
                    OpponentBadgeUrl = table.Column<string>(maxLength: 150, nullable: true),
                    OpponentDestructionPercentage = table.Column<double>(nullable: false),
                    OpponentLevel = table.Column<int>(nullable: false),
                    OpponentName = table.Column<string>(maxLength: 50, nullable: true),
                    OpponentStars = table.Column<int>(nullable: false),
                    OpponentTag = table.Column<string>(maxLength: 15, nullable: true),
                    Result = table.Column<string>(maxLength: 15, nullable: true),
                    Synced = table.Column<bool>(nullable: false),
                    TeamSize = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wars", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WarSyncs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true)
                        //.Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    AllianceMatches = table.Column<int>(nullable: false),
                    Finish = table.Column<DateTime>(nullable: false),
                    MissedStarts = table.Column<int>(nullable: false),
                    Start = table.Column<DateTime>(nullable: false),
                    WarMatches = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarSyncs", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 15, nullable: false),
                    BadgeUrl = table.Column<string>(maxLength: 150, nullable: true),
                    ClanRank = table.Column<int>(nullable: false),
                    ClanTag = table.Column<string>(maxLength: 15, nullable: true),
                    Donations = table.Column<int>(nullable: false),
                    DonationsReceived = table.Column<int>(nullable: false),
                    ExpLevel = table.Column<int>(nullable: false),
                    LeagueName = table.Column<string>(maxLength: 30, nullable: true),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Role = table.Column<string>(maxLength: 15, nullable: true),
                    Trophies = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Tag);
                    table.ForeignKey(
                        name: "FK_Members_Clans_ClanTag",
                        column: x => x.ClanTag,
                        principalTable: "Clans",
                        principalColumn: "Tag",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clans_Name",
                table: "Clans",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ClanValidities_ValidFrom",
                table: "ClanValidities",
                column: "ValidFrom");

            migrationBuilder.CreateIndex(
                name: "IX_ClanValidities_ValidTo",
                table: "ClanValidities",
                column: "ValidTo");

            migrationBuilder.CreateIndex(
                name: "IX_Members_ClanTag",
                table: "Members",
                column: "ClanTag");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Name",
                table: "Members",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Members_Role",
                table: "Members",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_ClanTag",
                table: "Wars",
                column: "ClanTag");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_EndTime",
                table: "Wars",
                column: "EndTime");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_OpponentTag_EndTime",
                table: "Wars",
                columns: new[] { "OpponentTag", "EndTime" });

            migrationBuilder.CreateIndex(
                name: "IX_WarSyncs_Finish",
                table: "WarSyncs",
                column: "Finish");

            migrationBuilder.CreateIndex(
                name: "IX_WarSyncs_Start",
                table: "WarSyncs",
                column: "Start");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanValidities");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "UpdateTasks");

            migrationBuilder.DropTable(
                name: "Wars");

            migrationBuilder.DropTable(
                name: "WarSyncs");

            migrationBuilder.DropTable(
                name: "Clans");
        }
    }
}
