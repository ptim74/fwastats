using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FWAStatsWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class SubmitRestrictionChangedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SubmitRestrictionChangedAt",
                table: "Clans",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubmitRestrictionChangedAt",
                table: "Clans");
        }
    }
}
