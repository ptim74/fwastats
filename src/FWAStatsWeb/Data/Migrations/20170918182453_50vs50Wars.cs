using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace FWAStatsWeb.Data.Migrations
{
    public partial class _50vs50Wars : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Base41",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base42",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base43",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base44",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base45",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base46",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base47",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base48",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base49",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Base50",
                table: "WeightResults",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Base41",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base42",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base43",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base44",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base45",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base46",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base47",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base48",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base49",
                table: "WeightResults");

            migrationBuilder.DropColumn(
                name: "Base50",
                table: "WeightResults");
        }
    }
}
