using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace LWFStatsWeb.Data.Migrations
{
    public partial class WeightResults : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeightResults",
                columns: table => new
                {
                    Tag = table.Column<string>(maxLength: 10, nullable: false),
                    Base01 = table.Column<int>(nullable: false),
                    Base02 = table.Column<int>(nullable: false),
                    Base03 = table.Column<int>(nullable: false),
                    Base04 = table.Column<int>(nullable: false),
                    Base05 = table.Column<int>(nullable: false),
                    Base06 = table.Column<int>(nullable: false),
                    Base07 = table.Column<int>(nullable: false),
                    Base08 = table.Column<int>(nullable: false),
                    Base09 = table.Column<int>(nullable: false),
                    Base10 = table.Column<int>(nullable: false),
                    Base11 = table.Column<int>(nullable: false),
                    Base12 = table.Column<int>(nullable: false),
                    Base13 = table.Column<int>(nullable: false),
                    Base14 = table.Column<int>(nullable: false),
                    Base15 = table.Column<int>(nullable: false),
                    Base16 = table.Column<int>(nullable: false),
                    Base17 = table.Column<int>(nullable: false),
                    Base18 = table.Column<int>(nullable: false),
                    Base19 = table.Column<int>(nullable: false),
                    Base20 = table.Column<int>(nullable: false),
                    Base21 = table.Column<int>(nullable: false),
                    Base22 = table.Column<int>(nullable: false),
                    Base23 = table.Column<int>(nullable: false),
                    Base24 = table.Column<int>(nullable: false),
                    Base25 = table.Column<int>(nullable: false),
                    Base26 = table.Column<int>(nullable: false),
                    Base27 = table.Column<int>(nullable: false),
                    Base28 = table.Column<int>(nullable: false),
                    Base29 = table.Column<int>(nullable: false),
                    Base30 = table.Column<int>(nullable: false),
                    Base31 = table.Column<int>(nullable: false),
                    Base32 = table.Column<int>(nullable: false),
                    Base33 = table.Column<int>(nullable: false),
                    Base34 = table.Column<int>(nullable: false),
                    Base35 = table.Column<int>(nullable: false),
                    Base36 = table.Column<int>(nullable: false),
                    Base37 = table.Column<int>(nullable: false),
                    Base38 = table.Column<int>(nullable: false),
                    Base39 = table.Column<int>(nullable: false),
                    Base40 = table.Column<int>(nullable: false),
                    PendingResult = table.Column<bool>(nullable: false),
                    TH10Count = table.Column<int>(nullable: false),
                    TH11Count = table.Column<int>(nullable: false),
                    TH7Count = table.Column<int>(nullable: false),
                    TH8Count = table.Column<int>(nullable: false),
                    TH9Count = table.Column<int>(nullable: false),
                    THSum = table.Column<int>(nullable: false),
                    Timestamp = table.Column<DateTime>(nullable: false),
                    Weight = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeightResults", x => x.Tag);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeightResults");
        }
    }
}
