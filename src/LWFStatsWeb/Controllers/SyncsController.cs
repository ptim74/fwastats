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

        public ActionResult Index(int? count)
        {
            var clans = new Dictionary<string, SyncIndexClan>();

            foreach (var clan in db.Clans.ToList())
            {
                var clanDetail = new SyncIndexClan();
                clanDetail.Tag = clan.Tag;
                clanDetail.Name = clan.Name;
                clanDetail.BadgeUrl = clan.BadgeUrl;
                clanDetail.Results = new List<SyncIndexResult>();
                clans.Add(clan.Tag, clanDetail);
            }

            var warsToTake = 3;
            if (count != null && count.HasValue)
                warsToTake = count.Value;

            var recentSyncs = db.WarSyncs.OrderByDescending(w => w.Start).Take(warsToTake).ToList();

            var earliestWar = DateTime.MaxValue;

            foreach (var s in recentSyncs)
            {
                if (s.Start < earliestWar)
                    earliestWar = s.Start;
            }

            earliestWar = earliestWar.AddDays(-2);

            var formerClans = (from f in db.ClanValidities
                              where f.ValidTo > earliestWar select f).ToDictionary(f => f.Tag);

            foreach(var formerClan in formerClans.Values)
            {
                if(!clans.ContainsKey(formerClan.Tag))
                {
                    var syncClan = new SyncIndexClan { Tag = formerClan.Tag, Name = formerClan.Name, Results = new List<SyncIndexResult>() };

                    var clanBadges = (from o in db.Wars
                                      where o.OpponentTag == formerClan.Tag
                                      orderby o.EndTime descending
                                      select o.OpponentBadgeUrl).ToList();

                    if (clanBadges.Count > 0)
                        syncClan.BadgeUrl = clanBadges.First();
                    
                    clans.Add(formerClan.Tag, syncClan);
                }
            }

            var warCount = 0;

            foreach (var s in recentSyncs)
            {
                var q = from w in db.Wars
                        where w.EndTime >= s.Start && w.EndTime <= s.Finish
                        select new { ClanTag = w.ClanTag, Result = w.Result, OpponentTag = w.OpponentTag, OpponentName = w.OpponentName, OpponentBadge = w.OpponentBadgeUrl };

                foreach (var r in q)
                {
                    if (clans.ContainsKey(r.ClanTag))
                    {
                        var clan = clans[r.ClanTag];
                        var isAlliance = false;
                        if(formerClans.ContainsKey(r.OpponentTag))
                        {
                            var opponentClan = formerClans[r.OpponentTag];
                            if (opponentClan.ValidFrom < s.Start && opponentClan.ValidTo > s.Start)
                                isAlliance = true;
                        }
                        clan.Results.Add(new SyncIndexResult()
                        {
                            Result = r.Result,
                            OpponentTag = r.OpponentTag,
                            OpponentName = r.OpponentName,
                            OpponentBadgeURL = r.OpponentBadge,
                            IsAlliance = isAlliance
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