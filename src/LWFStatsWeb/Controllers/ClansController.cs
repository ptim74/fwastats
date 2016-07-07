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
using LWFStatsWeb.Logic;

namespace LWFStatsWeb.Controllers
{
    public class ClansController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClashApi api;

        public ClansController(
            ApplicationDbContext db,
            IClashApi api)
        {
            this.db = db;
            this.api = api;
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

        public async Task<ActionResult> Details(string id)
        {
            var clan = new Clan();
            clan.Name = "Clan not found";
            clan.BadgeUrl = new ClanBadgeUrls();
            clan.Members = new List<Member>();
            try
            {
                var clans = db.Clans.Include(c => c.BadgeUrl).Include(c => c.Members).Where(c => c.Tag == id).ToList();
                if (clans.Count > 0)
                {
                    clan = clans.First();
                }
                else
                {
                    //Allow only other clans we have matched
                    var opponents = db.WarOpponents.Where(o => o.Tag == id).ToList();
                    if (opponents.Count > 0)
                    {
                        clan = await api.GetClan(id, false);
                    }
                }
            }
            catch(Exception e)
            {
                clan.Description = e.Message;
                
            }
            return View(clan);
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
                             select new { Tag = c.Tag, ClanName = c.Name, EndTime = w.EndTime, BadgeURL = b.Small };

                foreach (var extra in extraQ.ToList())
                {
                    clan.BadgeURL = extra.BadgeURL;
                    clan.LatestClan = extra.ClanName;
                    clan.LatestTag = extra.Tag;
                    clan.LatestDate = extra.EndTime.ToString("yyyy-MM-dd");
                }

                clans.Add(clan);
            }

            return View(clans);
        }
    }
}
