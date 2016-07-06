using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Models.SyncViewModels;
using LWFStatsWeb.Data;
using Microsoft.EntityFrameworkCore;

namespace LWFStatsWeb.Controllers
{
    public class SyncsController : Controller
    {
        private readonly ApplicationDbContext db;

        public SyncsController(ApplicationDbContext db)
        {
            this.db = db;
        }

        public ActionResult Index()
        {
            var clans = new Dictionary<string, SyncIndexClan>();

            foreach (var clan in db.Clans.Include(c => c.BadgeUrl).ToList())
            {
                var clanDetail = new SyncIndexClan();
                clanDetail.Tag = clan.Tag;
                clanDetail.Name = clan.Name;
                clanDetail.BadgeURL = clan.BadgeUrl.Small;
                clanDetail.Results = new List<SyncIndexResult>();
                clans.Add(clan.Tag, clanDetail);
            }

            var recentSyncs = db.WarSyncs.OrderByDescending(w => w.Start).Take(3).ToList();

            var warCount = 0;

            foreach (var s in recentSyncs)
            {
                var q = from w in db.Wars
                        join o in db.WarOpponents on w.ID equals o.WarID
                        join b in db.WarOpponentBadgeUrls on o.WarID equals b.WarID
                        where w.EndTime >= s.Start && w.EndTime <= s.Finish
                        select new { ClanTag = w.ClanTag, Result = w.Result, OpponentTag = o.Tag, OpponentName = o.Name, OpponentBadge = b.Small };

                foreach (var r in q)
                {
                    if (clans.ContainsKey(r.ClanTag))
                    {
                        var clan = clans[r.ClanTag];
                        clan.Results.Add(new SyncIndexResult()
                        {
                            Result = r.Result,
                            OpponentName = r.OpponentName,
                            OpponentBadgeURL = r.OpponentBadge,
                            IsAlliance = clans.ContainsKey(r.OpponentTag)
                        });
                    }
                }

                warCount++;

                foreach (var clan in clans.Values)
                {
                    if (clan.Results.Count() < warCount)
                    {
                        clan.Results.Add(new SyncIndexResult()
                        {
                            Result = "miss",
                            OpponentName = "",
                            IsAlliance = false
                        });
                    }
                }

            }

            var data = new IndexViewModel();

            data.Syncs = recentSyncs;

            data.Clans = clans.Values.OrderBy(c => c.Name).ToList();

            return View(data);
        }
    }
}