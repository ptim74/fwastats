using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models.HomeViewModels;
using Microsoft.EntityFrameworkCore;

namespace LWFStatsWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext db;

        public HomeController(
            ApplicationDbContext db)
        {
            this.db = db;
        }

        public IActionResult Index()
        {
            var model = new IndexViewModel();
            model.Counters = new CounterStats();
            model.LatestSync = new SyncStats();
            model.ClansNeedingHelp = new List<ClanDetails>();

            try
            {
                var clanTags = db.Clans.Select(c => c.Tag).ToList();

                model.Counters.ClanCount = clanTags.Count();
                model.Counters.MemberCount = db.Members.Count();
                model.Counters.WarCount = db.Wars.Count();
                model.Counters.SyncCount = db.WarSyncs.Count();

                var recentSyncs = db.WarSyncs.OrderByDescending(w => w.Start).Take(2).ToList();
                if(recentSyncs.Count > 1)
                {
                    var latestSync = recentSyncs.First();

                    if (latestSync.Finish > DateTime.Now.AddHours(-1))
                        latestSync = recentSyncs.Last();

                    model.LatestSync.Name = latestSync.Name;

                    var syncDate = latestSync.Start.AddDays(-2);

                    var validClanTags = (from f in db.ClanValidities
                                         where f.ValidTo > syncDate && f.ValidFrom < syncDate
                                         select f.Tag).ToList();

                    model.LatestSync.NotStarted = validClanTags.Count;

                    var wars = from w in db.Wars
                            where w.EndTime >= latestSync.Start && w.EndTime <= latestSync.Finish
                            select new { ClanTag = w.ClanTag, OpponentTag = w.OpponentTag };

                    foreach(var war in wars)
                    {
                        if(validClanTags.Contains(war.ClanTag))
                              model.LatestSync.NotStarted--;
                        if (validClanTags.Contains(war.OpponentTag))
                            model.LatestSync.AllianceMatches++;
                        else
                            model.LatestSync.WarMatches++;
                       }
                }

                var clansNeedingHelp = db.Clans.Where(c => c.Members >= 25).OrderBy(c => c.Members).Take(5);
                foreach(var clan in clansNeedingHelp)
                {
                    model.ClansNeedingHelp.Add(new ClanDetails { Tag = clan.Tag, Name = clan.Name, Members = clan.Members, BadgeUrl = clan.BadgeUrl });
                }
            }
            catch(Exception)
            {
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
