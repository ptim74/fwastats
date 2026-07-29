using FWAStatsWeb.Data;
using FWAStatsWeb.Logic;
using FWAStatsWeb.Models.HomeViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace FWAStatsWeb.Controllers;

public class HomeController : Controller
{
    private readonly ApplicationDbContext db;
    private readonly ILogger<HomeController> logger;
    readonly IGoogleSheetsService googleSheets;

    public HomeController(
        ApplicationDbContext db,
        ILogger<HomeController> logger,
         IGoogleSheetsService googleSheets)
    {
        this.db = db;
        this.logger = logger;
        this.googleSheets = googleSheets;
    }

    //[ResponseCache(Duration = Constants.CACHE_MIN)]
    public IActionResult Index()
    {
        logger.LogInformation("Index");

        var model = new IndexViewModel
        {
            Counters = new CounterStats(),
            LastStats = new Dictionary<int, SyncStats>(),
            SyncHistory = new List<SyncStats>(),
        };

        try
        {
            var validClans = db.Clans.Select(c => new { c.Tag, c.Group, c.Members, c.InLeague }).ToList();

            var recentSyncs = db.WarSyncs.Where(w => w.Finish < DateTime.UtcNow && w.Verified == true).OrderByDescending(w => w.Start).Take(10).ToList();

            var fromDate = recentSyncs.Last().Start;
            var loadedWars = (from w in db.Wars
                              where w.PreparationStartTime >= fromDate && w.Synced == true && w.Friendly == false && (w.TeamSize == Constants.WAR_SIZE2 || w.TeamSize == Constants.WAR_SIZE3)
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
                if (clan.InLeague)
                    model.Counters.ClansInLeague++;
            }

            var lastSync = recentSyncs.FirstOrDefault();

            if (lastSync != null)
                foreach (var teamSize in new int[] { Constants.WAR_SIZE3, Constants.WAR_SIZE2 })
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
                if (lastStat.Key == Constants.WAR_SIZE3)
                    model.Counters.TeamSize45Wars = lastStat.Value.AllianceMatches + lastStat.Value.WarMatches;
                else if (lastStat.Key == Constants.WAR_SIZE2)
                    model.Counters.TeamSize50Wars = lastStat.Value.AllianceMatches + lastStat.Value.WarMatches;
            }

            var totalWars = totalMatches + totalMismatches;
            if (totalWars > 0)
            {
                model.Counters.MatchPercentage = Math.Round(totalMatches * 100.0 / totalWars, 1);
                model.Counters.WinPercentage = Math.Round(totalWins * 100.0 / totalWars, 1);
            }
        }
        catch (Exception e)
        {
            logger.LogError("Index.Error: {Error}", e.ToString());
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

    //default 24h cache
    public IActionResult SubmitLimits()
    {
        logger.LogInformation("SubmitLimits");
        return View();
    }

    //[ResponseCache(Duration = Constants.CACHE_MIN)]
    public IActionResult Ping()
    {
        return Ok();
    }

    //[ResponseCache(Duration = Constants.CACHE_MIN)]
    public IActionResult DBPing()
    {
        db.Clans.FirstOrDefault();
        return Ok();
    }

    public IActionResult Error(int id)
    {
        logger.LogError("Error.{StatusCode}", id);

        try
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();
            logger.LogError("Error.Details: {Error}", feature.Error.ToString());
        }
        catch (Exception) { }

        ViewData["Message"] = "Sorry, an error occurred while processing your request.";

        if(id == 404)
            ViewData["Message"] = "Sorry, the page you are looking for could not be found.";

        return View();
    }

    public async Task<IActionResult> Tracker(string id)
    {
        var model = new TrackerViewModel();
        var tag = Utils.LinkIdToTag(id);

        logger.LogInformation("Tracker {ClanTag}", id);

        try
        {
            if (!string.IsNullOrEmpty(tag))
            {
                var clan = db.Clans.SingleOrDefault(c => c.Tag == tag);
                if (clan != null)
                {
                    var data = new List<IList<object>> { new List<object> { clan.LinkID } };
                    await googleSheets.Update(Constants.DONATION_TRACKER_SHEET_ID, "ROWS", "Setup!B2", data);
                    model.ClanName = clan.Name;
                    model.ClanTag = clan.Tag;
                }
            }
        }
        catch(Exception e)
        {
            logger.LogError("Tracker.Error: {Error}", e.ToString());
        }

        return View(model);
    }
}
