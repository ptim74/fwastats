using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using LWFStatsWeb.Models.ClanViewModels;

namespace LWFStatsWeb.Controllers
{
    public class ClansController : Controller
    {
        private readonly ApplicationDbContext db;

        public ClansController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // GET: Clans
        public ActionResult Index()
        {
            var clans = new List<ClanIndexClan>();

            var allWars = db.Wars.ToList();

            foreach (var clan in db.Clans.Include(c => c.BadgeUrl).ToList())
            {
                var clanDetail = new ClanIndexClan();
                clanDetail.Tag = clan.Tag;
                clanDetail.Name = clan.Name;
                clanDetail.Members = clan.MemberCount;
                clanDetail.BadgeURL = clan.BadgeUrl.Small;
                clanDetail.Results = new List<WarResult>();

                var recentWars = allWars.Where(w => w.ClanTag == clan.Tag).OrderByDescending(w => w.EndTime).Take(5).ToList();

                foreach (var recentWar in recentWars)
                {
                    var warResult = new WarResult();
                    warResult.Result = recentWar.Result;
                    clanDetail.Results.Add(warResult);
                }

                clans.Add(clanDetail);
            }

            return View(clans.OrderBy(c => c.Name).ToList());
        }

        public ActionResult Following()
        {
            var clans = new List<FollowingClan>();

            var followers = from o in db.WarOpponents
                            where !db.Clans.Any(c => c.Tag == o.Tag)
                            group o by new { o.Tag, o.Name } into grp
                            where grp.Count() > 2
                            orderby grp.Count() descending
                            select new { Tag = grp.Key.Tag, Name = grp.Key.Name, Count = grp.Count(), WarID = grp.Max(o => o.WarID) };

            foreach (var follower in followers.ToList())
            {
                var clan = new FollowingClan { Tag = follower.Tag, Name = follower.Name, Wars = follower.Count };

                var extraQ = from w in db.Wars
                             join b in db.WarOpponentBadgeUrls on w.ID equals b.WarID
                             join c in db.Clans on w.ClanTag equals c.Tag
                             where w.ID == follower.WarID
                             select new { ClanName = c.Name, EndTime = w.EndTime, BadgeURL = b.Small };

                foreach (var extra in extraQ.ToList())
                {
                    clan.BadgeURL = extra.BadgeURL;
                    clan.LatestClan = extra.ClanName;
                    clan.LatestDate = extra.EndTime.ToString("yyyy-MM-dd");
                }

                clans.Add(clan);
            }

            return View(clans);
        }
    }
}
