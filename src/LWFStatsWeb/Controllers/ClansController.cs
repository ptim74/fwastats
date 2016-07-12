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

        protected List<War> GetPrivateWars(string id)
        {
            var wars = new List<War>();
            var opponents = db.WarOpponents.Include(o => o.War.ClanResult).Include(o => o.War.OpponentResult).Where(o => o.Tag == id).ToList();
            if (opponents.Count > 0)
            {
                foreach (var o in opponents)
                {
                    var w = new War();
                    w.EndTime = o.War.EndTime;
                    w.OpponentResult = new WarOpponentResult();
                    w.OpponentResult.Stars = o.War.ClanResult.Stars;
                    w.OpponentResult.Name = o.War.ClanResult.Name;
                    w.ClanResult = new WarClanResult();
                    w.ClanResult.Stars = o.Stars;
                    if (o.War.Result == "win")
                        w.Result = "lose";
                    else if (o.War.Result == "lose")
                        w.Result = "win";
                    else
                        w.Result = o.War.Result;

                    wars.Add(w);
                }
            }
            return wars;
        }

        public async Task<Clan> GetDetails(string id)
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
                    if (clan.IsWarLogPublic)
                        clan.Wars = db.Wars.Include(w => w.ClanResult).Include(w => w.OpponentResult.BadgeUrl).Where(c => c.ClanTag == id).ToList();
                    else
                        clan.Wars = this.GetPrivateWars(id);
                }
                else
                {
                    var wars = this.GetPrivateWars(id);
                    if (wars.Count > 0)
                    {
                        clan = await api.GetClan(id, false);
                        clan.Wars = wars;
                    }
                }
            }
            catch (Exception e)
            {
                clan.Description = e.Message;

            }
            return clan;
        }

        public async Task<ActionResult> Details(string id)
        {
            var model = new IndexViewModel();
            model.InAlliance = db.Clans.Any(c => c.Tag == id);
            model.Clan = await this.GetDetails(id);
            model.Validity = await this.db.ClanValidities.SingleOrDefaultAsync(c => c.Tag == id);
            return View(model);
        }

        public ActionResult Following()
        {
            var clans = new List<FollowingClan>();

            var followers = from o in db.WarOpponents
                            where !db.Clans.Any(c => c.Tag == o.Tag)
                            where !db.ClanValidities.Any(v => v.Tag == o.Tag)
                            group o by new { o.Tag, o.Name } into grp
                            where grp.Count() > 1
                            //orderby grp.Count() descending
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

            return View(clans.OrderByDescending(c => c.LatestDate).ToList());
        }

        // GET: Clans/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var clanValidity = await db.ClanValidities.SingleOrDefaultAsync(m => m.Tag == id);
            if (clanValidity == null)
            {
                var opp = db.WarOpponents.FirstOrDefault(o => o.Tag == id);
                clanValidity = new ClanValidity() { Tag = id };
                if (opp != null)
                    clanValidity.Name = opp.Name;

                var opponents = (  from o in db.WarOpponents
                                   where o.Tag == id
                                   join w in db.Wars on o.WarID equals w.ID
                                   group w by new { o.Tag, o.Name } into g
                                   select new {
                                       Tag = g.Key.Tag,
                                       Name = g.Key.Name,
                                       MinEndTime = g.Min(w => w.EndTime),
                                       MaxEndTime = g.Max(w => w.EndTime) }).ToList();

                foreach(var opponent in opponents )
                {
                    clanValidity.Name = opponent.Name;
                    clanValidity.ValidFrom = opponent.MinEndTime.AddDays(-1);
                    clanValidity.ValidTo = opponent.MaxEndTime.AddDays(1);
                }

            }
            return View(clanValidity);
        }

        // POST: Clans/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Tag,ValidFrom,ValidTo")] ClanValidity clanValidity)
        {
            if (id != clanValidity.Tag)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var opp = db.WarOpponents.FirstOrDefault(o => o.Tag == id);
                    if (opp != null)
                    {
                        clanValidity.Name = opp.Name;

                        if (!this.ClanValidityExists(id))
                        {
                            if (clanValidity.ValidFrom < clanValidity.ValidTo)
                            {
                                db.Add(clanValidity);
                            }
                        }
                        else
                        {
                            if (clanValidity.ValidFrom == clanValidity.ValidTo)
                            {
                                db.Remove(clanValidity);
                            }
                            else if (clanValidity.ValidFrom < clanValidity.ValidTo)
                            {
                                db.Update(clanValidity);
                            }
                        }
                        await db.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClanValidityExists(clanValidity.Tag))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", new { id = id });
            }
            return View(clanValidity);
        }

        private bool ClanValidityExists(string id)
        {
            return db.ClanValidities.Any(e => e.Tag == id);
        }
    }
}
