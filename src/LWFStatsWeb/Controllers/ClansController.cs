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

            foreach (var clan in db.Clans.ToList())
            {
                var clanDetail = new ClanIndexClan();
                clanDetail.Tag = clan.Tag;
                clanDetail.Name = clan.Name;
                clanDetail.Members = clan.Members;
                clanDetail.BadgeUrl = clan.BadgeUrl;
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
            var opponentsWars = db.Wars.Where(o => o.OpponentTag == id).ToList();
            if (opponentsWars.Count > 0)
            {
                foreach (var o in opponentsWars)
                {
                    var w = new War();
                    w.ID = o.ID.Replace(o.ClanTag, o.OpponentTag);
                    w.EndTime = o.EndTime;
                    w.TeamSize = o.TeamSize;

                    w.ClanTag = o.OpponentTag;
                    w.ClanAttacks = 0;
                    w.ClanBadgeUrl = o.OpponentBadgeUrl;
                    w.ClanDestructionPercentage = o.OpponentDestructionPercentage;
                    w.ClanExpEarned = 0;
                    w.ClanLevel = o.OpponentLevel;
                    w.ClanName = o.OpponentName;
                    w.ClanStars = o.OpponentStars;

                    w.OpponentBadgeUrl = o.ClanBadgeUrl;
                    w.OpponentDestructionPercentage = o.ClanDestructionPercentage;
                    w.OpponentLevel = o.ClanLevel;
                    w.OpponentName = o.ClanName;
                    w.OpponentStars = o.ClanStars;
                    w.OpponentTag = o.ClanTag;

                    if (o.Result == "win")
                        w.Result = "lose";
                    else if (o.Result == "lose")
                        w.Result = "win";
                    else
                        w.Result = o.Result;

                    wars.Add(w);
                }
            }
            return wars;
        }

        protected async Task<Clan> GetDetails(string tag)
        {
            var clan = new Clan();
            clan.Name = "Clan not found";
            clan.MemberList = new List<Member>();
            try
            {
                var clans = db.Clans.Include(c => c.MemberList).Where(c => c.Tag == tag).ToList();
                if (clans.Count > 0)
                {
                    clan = clans.First();
                    if (clan.IsWarLogPublic)
                        clan.Wars = db.Wars.Where(c => c.ClanTag == tag).ToList();
                    else
                        clan.Wars = this.GetPrivateWars(tag);
                }
                else
                {
                    var wars = this.GetPrivateWars(tag);
                    if (wars.Count > 0)
                    {
                        clan = await api.GetClan(tag, false);
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

        protected string LinkIdToTag(string id)
        {
            return string.Concat("#", id.Replace("#", ""));
        }

        public async Task<ActionResult> Details(string id)
        {
            var tag = LinkIdToTag(id);
            var model = new IndexViewModel();
            model.InAlliance = db.Clans.Any(c => c.Tag == tag);
            model.Clan = await this.GetDetails(tag);
            model.Validity = await this.db.ClanValidities.SingleOrDefaultAsync(c => c.Tag == tag);
            return View(model);
        }

        public ActionResult Following()
        {
            var clans = new List<FollowingClan>();

            var followers = from o in db.Wars
                            where !db.ClanValidities.Any(v => v.Tag == o.OpponentTag)
                            group o by new { o.OpponentTag, o.OpponentName } into grp
                            where grp.Count() > 1
                            select new { Tag = grp.Key.OpponentTag, Name = grp.Key.OpponentName, Count = grp.Count(), WarID = grp.Max(o => o.ID) };

            foreach (var follower in followers.ToList())
            {
                var clan = new FollowingClan { Tag = follower.Tag, Name = follower.Name, Wars = follower.Count };

                var extraQ = from w in db.Wars
                             join v in db.ClanValidities on w.ClanTag equals v.Tag
                             where w.ID == follower.WarID
                             select new { Tag = v.Tag, ClanName = v.Name, EndTime = w.EndTime, BadgeURL = w.ClanBadgeUrl };

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
            var tag = LinkIdToTag(id);
            if (tag == null)
            {
                return NotFound();
            }

            var clanValidity = await db.ClanValidities.SingleOrDefaultAsync(m => m.Tag == tag);
            if (clanValidity == null)
            {
                var opp = db.Wars.FirstOrDefault(o => o.OpponentTag == tag);
                clanValidity = new ClanValidity() { Tag = tag };
                if (opp != null)
                    clanValidity.Name = opp.OpponentName;

                var opponents = (  from w in db.Wars
                                   where w.OpponentTag == tag
                                   group w by new { w.OpponentTag, w.OpponentName } into g
                                   select new {
                                       Tag = g.Key.OpponentTag,
                                       Name = g.Key.OpponentName,
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
        public async Task<IActionResult> Edit(string id, [Bind("Tag,LinkID,ValidFrom,ValidTo")] ClanValidity clanValidity)
        {
            var tag = LinkIdToTag(id);

            if (tag != clanValidity.Tag)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var opp = db.Wars.FirstOrDefault(o => o.OpponentTag == tag);
                    if (opp != null)
                    {
                        clanValidity.Name = opp.OpponentName;

                        if (!this.ClanValidityExists(tag))
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

                        var clan = await api.GetClan(tag, true);

                        if (clan.Wars != null)
                        {
                            var existingWars = db.Wars.Where(w => w.ClanTag == clan.Tag).ToDictionary(w => w.ID);
   
                            foreach (var clanWar in clan.Wars)
                                if (!existingWars.ContainsKey(clanWar.ID))
                                    if(clanWar.EndTime > clanValidity.ValidFrom && clanWar.EndTime < clanValidity.ValidTo)
                                        db.Wars.Add(clanWar);

                            foreach (var war in existingWars.Values)
                                if (war.EndTime > clanValidity.ValidTo || war.EndTime < clanValidity.ValidFrom)
                                    db.Entry(war).State = EntityState.Deleted;
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
