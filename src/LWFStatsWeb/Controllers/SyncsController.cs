using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Models.SyncViewModels;
using LWFStatsWeb.Data;
using Microsoft.EntityFrameworkCore;
using LWFStatsWeb.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using LWFStatsWeb.Logic;

namespace LWFStatsWeb.Controllers
{
    public class SyncsController : Controller
    {
        private readonly ApplicationDbContext db;
        private IMemoryCache memoryCache;
        ILogger<SyncsController> logger;

        public SyncsController(
            ApplicationDbContext db,
            IMemoryCache memoryCache,
            ILogger<SyncsController> logger)
        {
            this.db = db;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        protected IndexViewModel GetData(int? count)
        {
            var clans = new Dictionary<string, SyncIndexClan>();

            var warsToTake = 3;
            if (count != null && count.HasValue)
                warsToTake = count.Value;

            var blacklisted = db.BlacklistedClans.Select(c => c.Tag).ToDictionary(c => c);

            var recentSyncs = db.WarSyncs.Where(w => w.Start < Constants.MaxVisibleEndTime).OrderByDescending(w => w.Start).Take(warsToTake).ToList();

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

            /*
            foreach(var formerClan in formerClans.Values)
            {
                if (!clans.ContainsKey(formerClan.Tag))
                {
                    var syncClan = new SyncIndexClan { Tag = formerClan.Tag, Name = formerClan.Name, Results = new List<SyncIndexResult>(), Departed = true };

                    var clanBadges = (from o in db.Wars
                                        where o.OpponentTag == formerClan.Tag
                                        orderby o.EndTime descending
                                        select o.OpponentBadgeUrl).ToList();

                    if (clanBadges.Count > 0)
                        syncClan.BadgeUrl = clanBadges.First();

                    clans.Add(formerClan.Tag, syncClan);
                }
            }
            */

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

            var model = memoryCache.GetOrCreate(Constants.CACHE_SYNCS_ALL + count, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);
                return GetData(count);
            });

            return View(model);
        }

        protected DetailsViewModel GetWars(string id)
        {
            var model = new DetailsViewModel
            {
                Sync = db.WarSyncs.Where(s => s.Name == id).FirstOrDefault()
            };

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
                foreach (var war in db.Wars.Where(w => w.EndTime >= model.Sync.Start && w.EndTime <= model.Sync.Finish && w.Synced == true))
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

        [Route("Sync/{id}")]
        public ActionResult Details(string id)
        {
            logger.LogInformation("Details {0}", id);

            var model = memoryCache.GetOrCreate(Constants.CACHE_SYNC_DETAILS_ + id, entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);
                return GetWars(id);
            });

            return View(model);
        }
    }
}