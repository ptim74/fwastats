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

namespace LWFStatsWeb.Controllers
{
    public class SyncsController : Controller
    {
        private readonly ApplicationDbContext db;
        private IMemoryCache memoryCache;

        public SyncsController(
            ApplicationDbContext db,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.memoryCache = memoryCache;
        }

        protected IndexViewModel GetData(int? count)
        {
            var clans = new Dictionary<string, SyncIndexClan>();

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

            var clanQ = db.Clans.AsQueryable();
            var formerClanQ = db.ClanValidities.Where(v => v.ValidTo > earliestWar);
            var newClanQ = db.ClanValidities.Where(v => v.ValidFrom > earliestWar);

            foreach (var clan in clanQ)
            {
                var clanDetail = new SyncIndexClan();
                clanDetail.Tag = clan.Tag;
                clanDetail.Name = clan.Name;
                clanDetail.BadgeUrl = clan.BadgeUrl;
                clanDetail.Results = new List<SyncIndexResult>();
                clanDetail.HiddenLog = !clan.IsWarLogPublic;
                clans.Add(clan.Tag, clanDetail);
            }

            var formerClans = formerClanQ.ToDictionary(f => f.Tag);

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

            foreach (var newClan in newClanQ)
            {
                SyncIndexClan syncClan;
                if (clans.TryGetValue(newClan.Tag,out syncClan))
                {
                    syncClan.New = true;
                }
            }

            var warCount = 0;

            foreach (var s in recentSyncs)
            {
                var q = from w in db.Wars
                        where w.EndTime >= s.Start && w.EndTime <= s.Finish && w.Synced == true
                        select new { ClanTag = w.ClanTag, Result = w.Result, OpponentTag = w.OpponentTag, OpponentName = w.OpponentName, OpponentBadge = w.OpponentBadgeUrl };

                foreach (var r in q)
                {
                    SyncIndexClan clan;
                    if(clans.TryGetValue(r.ClanTag, out clan))
                    {
                        var isAlliance = false;
                        ClanValidity opponentClan;
                        if (formerClans.TryGetValue(r.OpponentTag, out opponentClan))
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

            return data;
        }

        public ActionResult Index(int? count)
        {
            var model = memoryCache.GetOrCreate("Syncs.All"+count, entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return GetData(count);
            });

            return View(model);
        }

        protected DetailsViewModel GetWars(string id)
        {
            var model = new DetailsViewModel();

            model.Sync = db.WarSyncs.Where(s => s.Name == id).FirstOrDefault();

            var searchTime = model.Sync.SearchTime;

            var validClans = new HashSet<string>();

            foreach(var validity in db.ClanValidities.Where(v => v.ValidTo > searchTime && v.ValidFrom < searchTime))
            {
                validClans.Add(validity.Tag);
            }

            model.Wars = new List<War>();
            var syncStart = model.Sync.Finish;

            foreach(var war in db.Wars.Where(w => w.EndTime >= model.Sync.Start && w.EndTime <= model.Sync.Finish && w.Synced == true))
            {
                if (validClans.Contains(war.ClanTag))
                {
                    model.Wars.Add(war);
                    if (war.EndTime < syncStart)
                        syncStart = war.EndTime;
                }
            }

            //Hide sync time
            foreach (var war in model.Wars)
                war.EndTime = new DateTime(2000, 1, 1).AddSeconds(war.EndTime.Subtract(syncStart).TotalSeconds);

            model.Wars = model.Wars.OrderBy(w => w.ClanName.ToLower()).ToList();
            return model;
        }

        public ActionResult Details(string id)
        {
            var model = memoryCache.GetOrCreate("Sync.All" + id, entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return GetWars(id);
            });

            return View(model);
        }
    }
}