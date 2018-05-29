using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models.HomeViewModels;
using Microsoft.Extensions.Logging;

namespace LWFStatsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;
        ILogger<HomeController> logger;

        public HomeController(
            ApplicationDbContext db,
            ILogger<HomeController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        [ResponseCache(Duration = Constants.CACHE_NORMAL)]
        public IActionResult Index()
        {
            logger.LogInformation("Index");

            var model = new IndexViewModel
            {
                Counters = new CounterStats(),
                LastStats = new Dictionary<int, SyncStats>(),
                SyncHistory = new List<SyncStats>(),
                TownhallCounters = new Dictionary<int, ICollection<TownhallCounter>>()
            };

            try
            {
                var validClans = db.Clans.Select(c => new { c.Tag, c.Group, c.Members }).ToList();

                var recentSyncs = db.WarSyncs.Where(w => w.Finish < DateTime.UtcNow && w.Verified == true).OrderByDescending(w => w.Start).Take(10).ToList();

                var fromDate = recentSyncs.Last().Start;
                var loadedWars = (from w in db.Wars
                                  where w.PreparationStartTime >= fromDate && w.Synced == true && w.Friendly == false && (w.TeamSize == Constants.WAR_SIZE1 || w.TeamSize == Constants.WAR_SIZE2)
                                  select new { w.Result, w.PreparationStartTime, w.ClanTag, w.OpponentTag, w.TeamSize }).ToList();

                var loadedValidities = db.ClanValidities.ToList();

                var totalWins = 0;
                var totalMatches = 0;
                var totalMismatches = 0;
                var totalNotStarted = 0;

                foreach (var clan in validClans)
                {
                    model.Counters.ClanCount++;
                    model.Counters.MemberCount += clan.Members;
                }

                var lastSync = recentSyncs.FirstOrDefault();

                if (lastSync != null)
                    foreach (var teamSize in new int[] { Constants.WAR_SIZE1, Constants.WAR_SIZE2 })
                        model.LastStats.Add(teamSize, new SyncStats { ID = lastSync.ID, DisplayName = lastSync.DisplayName });

                foreach (var currentSync in recentSyncs.OrderBy(w => w.Start))
                {
                    var syncDate = currentSync.Start;

                    var lastSyncWars = (from w in loadedWars
                                        where w.PreparationStartTime >= currentSync.Start && w.PreparationStartTime <= currentSync.Finish
                                        select new { w.Result, w.ClanTag, w.OpponentTag, w.TeamSize }).ToList();

                    var stats = new SyncStats
                    {
                        ID = currentSync.ID,
                        DisplayName = currentSync.DisplayName
                    };
                    var syncWins = 0;

                    var validClanTags = (from f in loadedValidities
                                         where f.ValidTo > syncDate && f.ValidFrom < syncDate
                                         select f.Tag).ToList();

                    var validOpponentTags = (from f in loadedValidities
                                             where f.ValidTo > syncDate && f.ValidFrom < syncDate
                                             select f.Tag).ToList();

                    stats.NotStarted = validClanTags.Count;

                    var statusPreparation = false;
                    var statusBattle = false;

                    foreach (var war in lastSyncWars)
                    {
                        if (war.Result == "preparation")
                            statusPreparation = true;
                        if (war.Result == "inWar")
                            statusBattle = true;
                    }

                    if (statusBattle)
                        stats.Status = "battle day";
                    else if (statusPreparation)
                        stats.Status = "preparation day";
                    else
                        stats.Status = "ended";

                    foreach (var war in lastSyncWars)
                    {
                        if (!model.LastStats.TryGetValue(war.TeamSize, out SyncStats lastStat))
                            lastStat = new SyncStats();

                        lastStat.Status = stats.Status;

                        if (validClanTags.Contains(war.ClanTag))
                        {
                            if (war.Result == "win")
                            {
                                syncWins++;
                            }
                            stats.NotStarted--;
                            if (validOpponentTags.Contains(war.OpponentTag))
                            {
                                stats.AllianceMatches++;
                                if (lastSync.ID == currentSync.ID)
                                    lastStat.AllianceMatches++;
                            }
                            else
                            {
                                stats.WarMatches++;
                                if (lastSync.ID == currentSync.ID)
                                    lastStat.WarMatches++;
                            }
                        }
                    }

                    if (stats.Status == "ended")
                    {
                        totalWins += syncWins;
                        totalMatches += stats.AllianceMatches;
                        totalNotStarted += stats.NotStarted;
                        totalMismatches += stats.WarMatches;
                    }

                    model.SyncHistory.Add(stats);
                }

                foreach (var lastStat in model.LastStats)
                {
                    if (lastStat.Key == Constants.WAR_SIZE1)
                        model.Counters.TeamSize40Wars = lastStat.Value.AllianceMatches + lastStat.Value.WarMatches;
                    else
                        model.Counters.TeamSize50Wars = lastStat.Value.AllianceMatches + lastStat.Value.WarMatches;
                }

                var totalWars = totalMatches + totalMismatches;
                if (totalWars > 0)
                {
                    model.Counters.MatchPercentage = Math.Round(totalMatches * 100.0 / totalWars, 1);
                    model.Counters.WinPercentage = Math.Round(totalWins * 100.0 / totalWars, 1);
                }

                foreach (var teamSize in new int[] { Constants.WAR_SIZE1, Constants.WAR_SIZE2 })
                {
                    var limitDate = DateTime.UtcNow.AddDays(-28);
                    var results = db.WeightResults.Where(r => r.Weight > 2000000 && r.TeamSize == teamSize && r.Timestamp > limitDate).ToList();
                    var divider = 50000;
                    var thcounters = new Dictionary<int, TownhallCounter>();
                    foreach (var result in results)
                    {
                        //rounding +/- 12500
                        var weight = (result.Weight + divider / 2) / divider;
                        if (!thcounters.TryGetValue(weight, out TownhallCounter th))
                        {
                            th = new TownhallCounter { Weight = weight };
                            thcounters.Add(weight, th);
                        }
                        th.Clans++;
                        th.TH11 += result.TH11Count;
                        th.TH10 += result.TH10Count;
                        th.TH9 += result.TH9Count;
                        th.TH8 += result.TH8Count;
                        th.TH8 += result.TH7Count;
                    }
                    foreach (var th in thcounters.Values)
                    {
                        th.Weight = th.Weight * divider / 1000;

                        th.TH10 = Math.Round(th.TH10 / th.Clans, 1);
                        th.TH9 = Math.Round(th.TH9 /= th.Clans, 1);
                        th.TH8 = Math.Round(th.TH8 /= th.Clans, 1);
                        th.TH11 = teamSize - th.TH10 - th.TH9 - th.TH8;
                    }

                    if (thcounters.Values.Count() > 0)
                        model.TownhallCounters.Add(teamSize, thcounters.Values.OrderBy(v => v.Weight).ToList());
                }
            }
            catch (Exception e)
            {
                logger.LogError("Index.Error: {0}", e.ToString());
            }

            return View(model);
        }

        //default 24h cache
        public IActionResult About()
        {
            logger.LogInformation("About");
            return View();
        }

        //default 24h cache
        public IActionResult PrivacyPolicy()
        {
            logger.LogInformation("PrivacyPolicy");
            return View();
        }

        [ResponseCache(Duration = Constants.CACHE_MIN)]
        public IActionResult Ping()
        {
            return Ok();
        }

        [ResponseCache(Duration = Constants.CACHE_MIN)]
        public IActionResult DBPing()
        {
            var test = db.Clans.FirstOrDefault();
            return Ok();
        }

        public IActionResult Error(int id)
        {
            logger.LogError("Error.{0}", id);

            ViewData["Message"] = "Sorry, an error occurred while processing your request.";

            if(id == 404)
                ViewData["Message"] = "Sorry, the page you are looking for could not be found.";

            return View();
        }

        public IActionResult Tracker()
        {
            logger.LogInformation("Tracker");
            return View();
        }
    }
}
