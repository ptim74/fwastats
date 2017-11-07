using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Models.SyncViewModels;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using Microsoft.Extensions.Logging;

namespace LWFStatsWeb.Controllers
{
    [ResponseCache(Duration = Constants.CACHE_NORMAL)]
    public class SyncsController : Controller
    {
        private readonly ApplicationDbContext db;
        ILogger<SyncsController> logger;

        public SyncsController(
            ApplicationDbContext db,
            ILogger<SyncsController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        protected IndexViewModel GetData(int? count)
        {
            var clans = new Dictionary<string, SyncIndexClan>();

            var warsToTake = 3;
            if (count != null && count.HasValue)
                warsToTake = count.Value;

            var blacklisted = db.BlacklistedClans.Select(c => c.Tag).ToDictionary(c => c);

            var recentSyncs = db.WarSyncs.Where(w => w.Start < Constants.MaxVisibleEndTime && w.Verified == true).OrderByDescending(w => w.Start).Take(warsToTake).ToList();

            var earliestWar = DateTime.MaxValue;

            foreach (var s in recentSyncs)
            {
                if (s.Start < earliestWar)
                    earliestWar = s.Start;
            }

            earliestWar = earliestWar.AddDays(-2);

            var clanQ = db.Clans.AsQueryable();
            var formerClanQ = db.ClanValidities.Where(v => v.ValidTo > earliestWar);
            var newClanQ = db.ClanValidities.Where(v => v.ValidFrom > earliestWar);

            foreach (var clan in clanQ)
            {
                var clanDetail = new SyncIndexClan
                {
                    Tag = clan.Tag,
                    Name = clan.Name,
                    BadgeUrl = clan.BadgeUrl,
                    Results = new List<SyncIndexResult>(),
                    HiddenLog = !clan.IsWarLogPublic
                };
                clans.Add(clan.Tag, clanDetail);
            }

            var formerClans = formerClanQ.ToDictionary(f => f.Tag);

            foreach (var newClan in newClanQ)
            {
                if (clans.TryGetValue(newClan.Tag, out SyncIndexClan syncClan))
                {
                    syncClan.New = true;
                }
            }

            var warCount = 0;

            foreach (var s in recentSyncs)
            {
                var q = from w in db.Wars
                        where w.EndTime >= s.Start && w.EndTime <= s.Finish && w.Synced == true && w.Friendly == false
                        select new { ClanTag = w.ClanTag, Result = w.Result, OpponentTag = w.OpponentTag, OpponentName = w.OpponentName, OpponentBadge = w.OpponentBadgeUrl };

                foreach (var r in q)
                {
                    if (clans.TryGetValue(r.ClanTag, out SyncIndexClan clan))
                    {
                        var isAlliance = false;
                        if (formerClans.TryGetValue(r.OpponentTag, out ClanValidity opponentClan))
                        {
                            if (opponentClan.ValidFrom < s.Start && opponentClan.ValidTo > s.Start)
                                isAlliance = true;
                        }
                        clan.Results.Add(new SyncIndexResult()
                        {
                            Result = r.Result,
                            OpponentTag = r.OpponentTag,
                            OpponentName = r.OpponentName,
                            OpponentBadgeURL = r.OpponentBadge,
                            OpponentIsBlacklisted = blacklisted.ContainsKey(r.OpponentTag),
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

            var data = new IndexViewModel
            {
                Syncs = recentSyncs,
                Clans = clans.Values.OrderBy(c => c.Name).ToList()
            };

            return data;
        }

        public ActionResult Index(int? count)
        {
            logger.LogInformation("Index {0}", count);

            var model = GetData(count);

            return View(model);
        }

        protected DetailsViewModel GetWars(int id, int teamSize)
        {
            var model = new DetailsViewModel
            {
                TeamSize = teamSize,
                Sync = db.WarSyncs.Where(s => s.ID == id && s.Verified == true && s.Start < Constants.MaxVisibleEndTime).FirstOrDefault()
            };

            if(model.Sync == null)
            {
                return null;
            }

            var blacklisted = db.BlacklistedClans.Select(c => c.Tag).ToList();

            var searchTime = model.Sync.SearchTime;

            var validClans = new HashSet<string>();

            foreach(var validity in db.ClanValidities.Where(v => v.ValidTo > searchTime && v.ValidFrom < searchTime))
            {
                validClans.Add(validity.Tag);
            }

            model.Wars = new List<War>();
            var syncStart = model.Sync.Finish;

            if (model.Sync.Start < Constants.MaxVisibleEndTime)
            {
                var wars = db.Wars.Where(w => w.EndTime >= model.Sync.Start && w.EndTime <= model.Sync.Finish && w.Synced == true);

                if (teamSize == Constants.WAR_SIZE1 || teamSize == Constants.WAR_SIZE2)
                    wars = wars.Where(w => w.TeamSize == teamSize);

                foreach (var war in wars)
                {
                    if (validClans.Contains(war.ClanTag))
                    {
                        model.Wars.Add(war);
                        if (war.EndTime < syncStart)
                            syncStart = war.EndTime;
                        if (blacklisted.Contains(war.OpponentTag))
                            war.Blacklisted = true;
                    }
                }
            }

            //Hide sync time
            foreach (var war in model.Wars)
                war.EndTime = new DateTime(2000, 1, 1).AddSeconds(war.EndTime.Subtract(syncStart).TotalSeconds);

            model.Wars = model.Wars.OrderBy(w => w.ClanName.ToLower()).ToList();
            return model;
        }

        [Route("Sync/{id}/{teamSize?}")]
        public ActionResult Details(string id, int teamSize)
        {
            logger.LogInformation("Details {0} {1}", id, teamSize);

            if (int.TryParse(id, out int i))
            {
                var model = GetWars(i, teamSize);

                if(model == null)
                    return NotFound($"Sync {id} not found.");

                return View(model);
            }
            else //Support old yyyy-mm-dd style syncs, google bot is still requesting these
            {
                return NotFound($"Sync {id} not found.");
            }
        }
    }
}