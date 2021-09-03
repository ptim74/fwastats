using Microsoft.EntityFrameworkCore.Migrations;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class SubmitLogCookie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cookie",
                table: "SubmitLogs",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SubmitLogs_Cookie_Modified",
                table: "SubmitLogs",
                columns: new[] { "Cookie", "Modified" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SubmitLogs_Cookie_Modified",
                table: "SubmitLogs");

            migrationBuilder.DropColumn(
                name: "Cookie",
                table: "SubmitLogs");
        }
    }
}
