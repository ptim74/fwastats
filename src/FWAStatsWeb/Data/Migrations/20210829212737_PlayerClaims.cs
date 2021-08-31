using Microsoft.EntityFrameworkCore.Migrations;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class PlayerClaims : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerClaims",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 15, nullable: false),
                    UserId = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerClaims", x => x.Tag);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerClaims_UserId",
                table: "PlayerClaims",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerClaims");
        }
    }
}
