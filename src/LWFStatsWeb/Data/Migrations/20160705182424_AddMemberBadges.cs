using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class AddMemberBadges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MemberBadgeUrls",
                columns: table => new
                {
                    MemberTag = table.Column<string>(maxLength: 10, nullable: false),
                    Large = table.Column<string>(maxLength: 150, nullable: true),
                    Medium = table.Column<string>(maxLength: 150, nullable: true),
                    Small = table.Column<string>(maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberBadgeUrls", x => x.MemberTag);
                    table.ForeignKey(
                        name: "FK_MemberBadgeUrls_Members_MemberTag",
                        column: x => x.MemberTag,
                        principalTable: "Members",
                        principalColumn: "Tag",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MemberBadgeUrls_MemberTag",
                table: "MemberBadgeUrls",
                column: "MemberTag",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MemberBadgeUrls");
        }
    }
}
