using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models.HomeViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace LWFStatsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;
        private IMemoryCache memoryCache;

        public HomeController(
            ApplicationDbContext db,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.memoryCache = memoryCache;
        }

        public IActionResult Index()
        {
            var CACHEKEY = "Home.Index";
            IndexViewModel model;

            if (!memoryCache.TryGetValue<IndexViewModel>(CACHEKEY, out model))
            {
                model = new IndexViewModel();
                model.Counters = new List<CounterStats>();
                model.LastSyncs = new List<SyncStats>();
                model.SyncHistories = new List<SyncHistory>();

                try
                {
                    var validClans = db.Clans.Select(c => new { c.Tag, c.Group, c.Members }).ToList();

                    var recentSyncs = db.WarSyncs.OrderByDescending(w => w.Start).Take(10).ToList();

                    var fromDate = recentSyncs.Last().Start;
                    var loadedWars = (from w in db.Wars
                                      where w.EndTime >= fromDate
                                      select new { Result = w.Result, EndTime = w.EndTime, ClanTag = w.ClanTag, OpponentTag = w.OpponentTag }).ToList();

                    var loadedValidities = db.ClanValidities.ToList();

                        var counters = new CounterStats();

                        var totalWins = 0;
                        var totalMatches = 0;
                        var totalMismatches = 0;
                        var totalNotStarted = 0;

                        foreach (var clan in validClans)
                        {
                            counters.ClanCount++;
                            counters.MemberCount += clan.Members;
                        }

                        var syncHistory = new SyncHistory { Syncs = new List<SyncStats>() };

                        foreach (var latestSync in recentSyncs.OrderBy(w => w.Start))
                        {
                            var syncDate = latestSync.Start.AddDays(-2);

                            var lastSyncWars = (from w in loadedWars
                                                where w.EndTime >= latestSync.Start && w.EndTime <= latestSync.Finish
                                                select new { Result = w.Result, ClanTag = w.ClanTag, OpponentTag = w.OpponentTag }).ToList();

                            var stats = new SyncStats();
                            stats.Name = latestSync.Name;

                        var validClanTags = (from f in loadedValidities
                                                 where f.ValidTo > syncDate && f.ValidFrom < syncDate
                                                 select f.Tag).ToList();

                            var validOpponentTags = (from f in loadedValidities
                                                     where f.ValidTo > syncDate && f.ValidFrom < syncDate
                                                     select f.Tag).ToList();

                            stats.NotStarted = validClanTags.Count;

                            foreach (var war in lastSyncWars)
                            {
                                if (validClanTags.Contains(war.ClanTag))
                                {
                                    if (war.Result == "win")
                                    {
                                        totalWins++;
                                    }
                                    stats.NotStarted--;
                                    if (validOpponentTags.Contains(war.OpponentTag))
                                        stats.AllianceMatches++;
                                    else
                                        stats.WarMatches++;
                                }
                            }

                            totalMatches += stats.AllianceMatches;
                            totalNotStarted += stats.NotStarted;
                            totalMismatches += stats.WarMatches;

                            syncHistory.Syncs.Add(stats);
                        }

                        if (syncHistory.Syncs.Count > 0)
                        {
                            model.SyncHistories.Add(syncHistory);
                            var lastSync = syncHistory.Syncs.Last();
                            model.LastSyncs.Add(new SyncStats
                            {
                                LastSyncName = lastSync.Name,
                                AllianceMatches = lastSync.AllianceMatches,
                                WarMatches = lastSync.WarMatches,
                                NotStarted = lastSync.NotStarted
                            });
                        }

                        var totalWars = totalMatches + totalMismatches;
                        if (totalWars > 0)
                        {
                            counters.MatchPercentage = Math.Round(totalMatches * 100.0 / totalWars, 1);
                            counters.WinPercentage = Math.Round(totalWins * 100.0 / totalWars, 1);
                        }

                        model.Counters.Add(counters);

                    memoryCache.Set<IndexViewModel>(CACHEKEY, model, 
                        new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(20)));
                }
                catch (Exception)
                {
                }
            }

            return View(model);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
