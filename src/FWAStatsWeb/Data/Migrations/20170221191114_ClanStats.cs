using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class ClanStats : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles");

            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedWeight",
                table: "Clans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "MatchPercentage",
                table: "Clans",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "Th10Count",
                table: "Clans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Th11Count",
                table: "Clans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Th8Count",
                table: "Clans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Th9Count",
                table: "Clans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ThLowCount",
                table: "Clans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WarCount",
                table: "Clans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "WinPercentage",
                table: "Clans",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "EstimatedWeight",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "MatchPercentage",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "Th10Count",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "Th11Count",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "Th8Count",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "Th9Count",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "ThLowCount",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "WarCount",
                table: "Clans");

            migrationBuilder.DropColumn(
                name: "WinPercentage",
                table: "Clans");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");
        }
    }
}
