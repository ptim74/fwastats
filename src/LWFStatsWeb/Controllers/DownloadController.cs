using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using System.IO;
using LWFStatsWeb.Data;

namespace LWFStatsWeb.Controllers
{
    public class DownloadController : Controller
    {
        private readonly ApplicationDbContext db;

        public DownloadController(
            ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Clans()
        {
            var row = 1;
            var col = 1;

            ExcelPackage pck = new ExcelPackage();

            var ws = pck.Workbook.Worksheets.Add("Clans");
            ws.Row(row).Style.Font.Bold = true;

            ws.Cells[row, col++].Value = "Tag";
            ws.Cells[row, col++].Value = "Name";
            ws.Cells[row, col++].Value = "ClanLevel";
            ws.Cells[row, col++].Value = "ClanPoints";
            ws.Cells[row, col++].Value = "Description";
            ws.Cells[row, col++].Value = "Group";
            ws.Cells[row, col++].Value = "IsWarLogPublic";
            ws.Cells[row, col++].Value = "LocationName";
            ws.Cells[row, col++].Value = "Members";
            ws.Cells[row, col++].Value = "RequiredTrophies";
            ws.Cells[row, col++].Value = "Type";
            ws.Cells[row, col++].Value = "WarFrequency";
            ws.Cells[row, col++].Value = "WarLosses";
            ws.Cells[row, col++].Value = "WarTies";
            ws.Cells[row, col++].Value = "WarWins";
            ws.Cells[row, col++].Value = "WarWinStreak";

            var clanNames = new Dictionary<string, string>();

            foreach (var clan in db.Clans)
            {
                row++;
                col = 1;
                ws.Cells[row, col++].Value = clan.Tag;
                ws.Cells[row, col++].Value = clan.Name;
                ws.Cells[row, col++].Value = clan.ClanLevel;
                ws.Cells[row, col++].Value = clan.ClanPoints;
                ws.Cells[row, col++].Value = clan.Description;
                ws.Cells[row, col++].Value = clan.Group;
                ws.Cells[row, col++].Value = clan.IsWarLogPublic;
                ws.Cells[row, col++].Value = clan.LocationName;
                ws.Cells[row, col++].Value = clan.Members;
                ws.Cells[row, col++].Value = clan.RequiredTrophies;
                ws.Cells[row, col++].Value = clan.Type;
                ws.Cells[row, col++].Value = clan.WarFrequency;
                ws.Cells[row, col++].Value = clan.WarLosses;
                ws.Cells[row, col++].Value = clan.WarTies;
                ws.Cells[row, col++].Value = clan.WarWins;
                ws.Cells[row, col++].Value = clan.WarWinStreak;

                clanNames.Add(clan.Tag, clan.Name);
            }

            row = 1;
            col = 1;

            ws = pck.Workbook.Worksheets.Add("Members");

            ws.Row(row).Style.Font.Bold = true;

            ws.Cells[row, col++].Value = "ClanTag";
            ws.Cells[row, col++].Value = "ClanName";
            ws.Cells[row, col++].Value = "Tag";
            ws.Cells[row, col++].Value = "Name";
            ws.Cells[row, col++].Value = "ClanRank";
            ws.Cells[row, col++].Value = "Donations";
            ws.Cells[row, col++].Value = "DonationsReceived";
            ws.Cells[row, col++].Value = "ExpLevel";
            ws.Cells[row, col++].Value = "LeagueName";
            ws.Cells[row, col++].Value = "Role";
            ws.Cells[row, col++].Value = "Trophies";

            foreach (var member in from m in db.Members orderby m.ClanTag, m.ClanRank select m)
            {
                var clanName = "";
                if (clanNames.ContainsKey(member.ClanTag))
                    clanName = clanNames[member.ClanTag];
                row++;
                col = 1;
                ws.Cells[row, col++].Value = member.ClanTag;
                ws.Cells[row, col++].Value = clanName;
                ws.Cells[row, col++].Value = member.Tag;
                ws.Cells[row, col++].Value = member.Name;
                ws.Cells[row, col++].Value = member.ClanRank;
                ws.Cells[row, col++].Value = member.Donations;
                ws.Cells[row, col++].Value = member.DonationsReceived;
                ws.Cells[row, col++].Value = member.ExpLevel;
                ws.Cells[row, col++].Value = member.LeagueName;
                ws.Cells[row, col++].Value = member.Role;
                ws.Cells[row, col++].Value = member.Trophies;
            }

            var memorystream = new MemoryStream();
            pck.SaveAs(memorystream);
            memorystream.Position = 0;
            return new FileStreamResult(memorystream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "FWAStats_Clans.xlsx" };
        }

        public IActionResult Wars()
        {
            var row = 1;
            var col = 1;

            var mindate = DateTime.MinValue.AddDays(1);
            var maxdate = DateTime.UtcNow;

            ExcelPackage pck = new ExcelPackage();

            var ws = pck.Workbook.Worksheets.Add("Clans");
            ws.Row(row).Style.Font.Bold = true;

            ws.Cells[row, col++].Value = "Tag";
            ws.Cells[row, col++].Value = "Name";
            ws.Cells[row, col++].Value = "Group";
            ws.Cells[row, col++].Value = "ValidFrom";
            ws.Cells[row, col++].Value = "ValidTo";

            var activeClans = db.Clans.Select(c => c.Tag).ToList();

            foreach (var clan in db.ClanValidities.OrderBy(c => c.Name))
            {
                row++;
                col = 1;
                ws.Cells[row, col++].Value = clan.Tag;
                ws.Cells[row, col++].Value = clan.Name;
                ws.Cells[row, col++].Value = clan.Group;
                ws.Cells[row, col].Style.Numberformat.Format = "yyyy-mm-dd";
                if(clan.ValidFrom > mindate)
                    ws.Cells[row, col].Value = clan.ValidFrom;
                col++;
                ws.Cells[row, col].Style.Numberformat.Format = "yyyy-mm-dd";
                if (clan.ValidTo < maxdate && !activeClans.Contains(clan.Tag))
                    ws.Cells[row, col].Value = clan.ValidTo;

            }

            row = 1;
            col = 1;

            ws = pck.Workbook.Worksheets.Add("Syncs");
            ws.Row(row).Style.Font.Bold = true;

            ws.Cells[row, col++].Value = "Start";
            ws.Cells[row, col++].Value = "Finish";
            ws.Cells[row, col++].Value = "Duration";
            ws.Cells[row, col++].Value = "AllianceMatches";
            ws.Cells[row, col++].Value = "WarMatches";
            ws.Cells[row, col++].Value = "MissedStarts";

            foreach (var sync in db.WarSyncs.OrderByDescending(s => s.Start))
            {
                row++;
                col = 1;
                ws.Cells[row, col].Style.Numberformat.Format = "yyyy-mm-dd HH:MM";
                ws.Cells[row, col++].Value = sync.Start;
                ws.Cells[row, col].Style.Numberformat.Format = "yyyy-mm-dd HH:MM";
                ws.Cells[row, col++].Value = sync.Finish;
                ws.Cells[row, col].Style.Numberformat.Format = "HH:MM";
                ws.Cells[row, col++].Value = sync.Duration;
                ws.Cells[row, col++].Value = sync.AllianceMatches;
                ws.Cells[row, col++].Value = sync.WarMatches;
                ws.Cells[row, col++].Value = sync.MissedStarts;
            }

            row = 1;
            col = 1;

            ws = pck.Workbook.Worksheets.Add("Wars");
            ws.Row(row).Style.Font.Bold = true;

            ws.Cells[row, col++].Value = "EndTime";
            ws.Cells[row, col++].Value = "Synced";
            ws.Cells[row, col++].Value = "Matched";
            ws.Cells[row, col++].Value = "TeamSize";
            ws.Cells[row, col++].Value = "Result";
            ws.Cells[row, col++].Value = "ClanTag";
            ws.Cells[row, col++].Value = "ClanName";
            ws.Cells[row, col++].Value = "ClanAttacks";
            ws.Cells[row, col++].Value = "ClanExpEarned";
            ws.Cells[row, col++].Value = "ClanLevel";
            ws.Cells[row, col++].Value = "ClanStars";
            ws.Cells[row, col++].Value = "ClanDestructionPercentage";
            ws.Cells[row, col++].Value = "OpponentTag";
            ws.Cells[row, col++].Value = "OpponentName";
            ws.Cells[row, col++].Value = "OpponentLevel";
            ws.Cells[row, col++].Value = "OpponentStars";
            ws.Cells[row, col++].Value = "OpponentDestructionPercentage";

            foreach (var war in db.Wars.OrderByDescending(s => s.EndTime))
            {
                row++;
                col = 1;
                ws.Cells[row, col].Style.Numberformat.Format = "yyyy-mm-dd HH:MM";
                ws.Cells[row, col++].Value = war.EndTime;
                ws.Cells[row, col++].Value = war.Synced;
                ws.Cells[row, col++].Value = war.Matched;
                ws.Cells[row, col++].Value = war.TeamSize;
                ws.Cells[row, col++].Value = war.Result;
                ws.Cells[row, col++].Value = war.ClanTag;
                ws.Cells[row, col++].Value = war.ClanName;
                ws.Cells[row, col++].Value = war.ClanAttacks;
                ws.Cells[row, col++].Value = war.ClanExpEarned;
                ws.Cells[row, col++].Value = war.ClanLevel;
                ws.Cells[row, col++].Value = war.ClanStars;
                ws.Cells[row, col++].Value = war.ClanDestructionPercentage;
                ws.Cells[row, col++].Value = war.OpponentTag;
                ws.Cells[row, col++].Value = war.OpponentName;
                ws.Cells[row, col++].Value = war.OpponentLevel;
                ws.Cells[row, col++].Value = war.OpponentStars;
                ws.Cells[row, col++].Value = war.OpponentDestructionPercentage;
            }

            var memorystream = new MemoryStream();
            pck.SaveAs(memorystream);
            memorystream.Position = 0;
            return new FileStreamResult(memorystream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") { FileDownloadName = "FWAStats_Wars.xlsx" };
        }
    }
}