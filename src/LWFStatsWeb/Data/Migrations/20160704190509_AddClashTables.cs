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
                    Tag = table.Column<string>(maxLength: 10, nullable: false),
                    ClanLevel = table.Column<int>(nullable: false),
                    ClanPoints = table.Column<int>(nullable: false),
                    ClanType = table.Column<string>(maxLength: 10, nullable: true),
                    Description = table.Column<string>(maxLength: 300, nullable: true),
                    IsWarLogPublic = table.Column<bool>(nullable: false),
                    MemberCount = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    RequiredTrophies = table.Column<int>(nullable: false),
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
                name: "UpdateTasks",
                columns: table => new
                {
                    ID = table.Column<Guid>(nullable: false),
                    ClanName = table.Column<string>(maxLength: 50, nullable: true),
                    ClanTag = table.Column<string>(maxLength: 10, nullable: true),
                    Mode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UpdateTasks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WarSyncs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
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
                name: "ClanBadgeUrls",
                columns: table => new
                {
                    ClanTag = table.Column<string>(maxLength: 10, nullable: false),
                    Large = table.Column<string>(maxLength: 150, nullable: true),
                    Medium = table.Column<string>(maxLength: 150, nullable: true),
                    Small = table.Column<string>(maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClanBadgeUrls", x => x.ClanTag);
                    table.ForeignKey(
                        name: "FK_ClanBadgeUrls_Clans_ClanTag",
                        column: x => x.ClanTag,
                        principalTable: "Clans",
                        principalColumn: "Tag",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 10, nullable: false),
                    ClanRank = table.Column<int>(nullable: false),
                    ClanTag = table.Column<string>(maxLength: 10, nullable: true),
                    Donations = table.Column<int>(nullable: false),
                    DonationsReceived = table.Column<int>(nullable: false),
                    ExpLevel = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Role = table.Column<string>(maxLength: 10, nullable: true),
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

            migrationBuilder.CreateTable(
                name: "Wars",
                columns: table => new
                {
                    ID = table.Column<string>(maxLength: 30, nullable: false),
                    ClanTag = table.Column<string>(maxLength: 10, nullable: true),
                    EndTime = table.Column<DateTime>(nullable: false),
                    Result = table.Column<string>(maxLength: 10, nullable: true),
                    TeamSize = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wars", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Wars_Clans_ClanTag",
                        column: x => x.ClanTag,
                        principalTable: "Clans",
                        principalColumn: "Tag",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WarParticipants",
                columns: table => new
                {
                    WarID = table.Column<string>(maxLength: 30, nullable: false),
                    Attacks = table.Column<int>(nullable: false),
                    ClanLevel = table.Column<int>(nullable: false),
                    DestructionPercentage = table.Column<double>(nullable: false),
                    ExpEarned = table.Column<int>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Stars = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarParticipants", x => x.WarID);
                    table.ForeignKey(
                        name: "FK_WarParticipants_Wars_WarID",
                        column: x => x.WarID,
                        principalTable: "Wars",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarOpponents",
                columns: table => new
                {
                    WarID = table.Column<string>(maxLength: 30, nullable: false),
                    ClanLevel = table.Column<int>(nullable: false),
                    DestructionPercentage = table.Column<double>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    Stars = table.Column<int>(nullable: false),
                    Tag = table.Column<string>(maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarOpponents", x => x.WarID);
                    table.ForeignKey(
                        name: "FK_WarOpponents_Wars_WarID",
                        column: x => x.WarID,
                        principalTable: "Wars",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WarOpponentBadgeUrls",
                columns: table => new
                {
                    WarID = table.Column<string>(maxLength: 30, nullable: false),
                    Large = table.Column<string>(maxLength: 150, nullable: true),
                    Medium = table.Column<string>(maxLength: 150, nullable: true),
                    Small = table.Column<string>(maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WarOpponentBadgeUrls", x => x.WarID);
                    table.ForeignKey(
                        name: "FK_WarOpponentBadgeUrls_WarOpponents_WarID",
                        column: x => x.WarID,
                        principalTable: "WarOpponents",
                        principalColumn: "WarID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClanBadgeUrls_ClanTag",
                table: "ClanBadgeUrls",
                column: "ClanTag",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_ClanTag",
                table: "Members",
                column: "ClanTag");

            migrationBuilder.CreateIndex(
                name: "IX_Wars_ClanTag",
                table: "Wars",
                column: "ClanTag");

            migrationBuilder.CreateIndex(
                name: "IX_WarParticipants_WarID",
                table: "WarParticipants",
                column: "WarID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarOpponentBadgeUrls_WarID",
                table: "WarOpponentBadgeUrls",
                column: "WarID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WarOpponents_WarID",
                table: "WarOpponents",
                column: "WarID",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClanBadgeUrls");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "UpdateTasks");

            migrationBuilder.DropTable(
                name: "WarParticipants");

            migrationBuilder.DropTable(
                name: "WarOpponentBadgeUrls");

            migrationBuilder.DropTable(
                name: "WarSyncs");

            migrationBuilder.DropTable(
                name: "WarOpponents");

            migrationBuilder.DropTable(
                name: "Wars");

            migrationBuilder.DropTable(
                name: "Clans");
        }
    }
}
