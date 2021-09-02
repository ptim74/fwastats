using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class SubmitLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SubmitLogs",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tag = table.Column<string>(maxLength: 15, nullable: true),
                    Modified = table.Column<DateTime>(nullable: false),
                    IpAddr = table.Column<string>(maxLength: 50, nullable: true),
                    Changes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubmitLogs", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SubmitLogs_IpAddr_Modified",
                table: "SubmitLogs",
                columns: new[] { "IpAddr", "Modified" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SubmitLogs");
        }
    }
}
