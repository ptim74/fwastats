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
using System.IO;

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

        public IndexViewModel GetClanList(string filter)
        {
            var clans = new IndexViewModel { Group = filter };

            var wars = (from w in db.Wars.Select(w => new { w.ClanTag, w.Synced })
                        where w.Synced == true
                        group w by w.ClanTag into g
                        select new { Tag = g.Key, Count = g.Count() }).ToDictionary(w => w.Tag, w => w.Count);

            var wins = (from w in db.Wars.Select(w => new { w.ClanTag, w.Synced, w.Result })
                        where w.Synced == true && w.Result == "win"
                        group w by w.ClanTag into g
                        select new { Tag = g.Key, Count = g.Count() }).ToDictionary(w => w.Tag, w => w.Count);

            var matches = (from w in db.Wars.Select(w => new { w.ClanTag, w.Synced, w.Matched })
                           where w.Synced == true && w.Matched == true
                           group w by w.ClanTag into g
                           select new { Tag = g.Key, Count = g.Count() }).ToDictionary(w => w.Tag, w => w.Count);

            var clanQ = db.Clans.Select(c => new ClanIndexClan
            {
                Tag = c.Tag,
                Name = c.Name,
                Members = c.Members,
                BadgeUrl = c.BadgeUrl,
                Group = c.Group
            });

            if (filter.Equals("FWA"))
                clanQ = clanQ.Where(c => c.Group == filter);
            if (filter.Equals("FWAL"))
                clanQ = clanQ.Where(c => c.Group == filter || c.Group == "LWF");

            foreach (var clan in clanQ.OrderBy(c => c.Name.ToLower()))
            {
                int warCount, winCount, matchCount;

                if (wars.TryGetValue(clan.Tag, out warCount))
                    clan.WarCount = warCount;

                if(clan.WarCount > 0)
                {
                    if (wins.TryGetValue(clan.Tag, out winCount))
                        clan.WinPercentage = winCount * 100 / clan.WarCount;
                    if (matches.TryGetValue(clan.Tag, out matchCount))
                        clan.MatchPercentage = matchCount * 100 / clan.WarCount;
                }

                clans.Add(clan);
            }

            return clans;
        }

        // GET: Clans
        public ActionResult Index()
        {
            return View(GetClanList(""));
        }

        public ActionResult FWA()
        {
            return View("Index", GetClanList("FWA"));
        }

        public ActionResult FWAL()
        {
            return View("Index", GetClanList("FWAL"));
        }

        public ActionResult Departed()
        {
            var clans = new List<FormerClan>();

            var clanQ = from c in db.ClanValidities where c.ValidTo < DateTime.Now orderby c.Name.ToLower() select c;

            var clanBadges = (from w in db.Wars group w by w.OpponentTag into g select new { Tag = g.Key, BadgeUrl = g.Max(w => w.OpponentBadgeUrl) }).ToDictionary(w => w.Tag, w => w.BadgeUrl);

            foreach(var clan in clanQ)
            {
                var clanDetail = new FormerClan();
                clanDetail.Tag = clan.Tag;
                clanDetail.Name = clan.Name;
                clanDetail.Group = clan.Group;
                clanDetail.ValidFrom = clan.ValidFrom;
                clanDetail.ValidTo = clan.ValidTo;

                string badgeUrl;

                if (clanBadges.TryGetValue(clan.Tag, out badgeUrl))
                    clanDetail.BadgeURL = badgeUrl;

                clans.Add(clanDetail);
            }

            return View(clans);
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
                    w.Matched = o.Matched;
                    w.Synced = o.Synced;

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
            clan.Tag = tag;
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
            var model = new DetailsViewModel();
            model.InAlliance = db.Clans.Any(c => c.Tag == tag);
            model.Clan = await this.GetDetails(tag);
            model.Validity = await this.db.ClanValidities.SingleOrDefaultAsync(c => c.Tag == tag);
            return View(model);
        }

        public ActionResult Following()
        {
            var clans = new Dictionary<string, FollowingClan>();

            var mismatches = from w in db.Wars where w.Synced == true && w.Matched == false orderby w.ID select w;

            foreach(var mismatch in mismatches)
            {
                FollowingClan followingClan = null;
                if (!clans.TryGetValue(mismatch.OpponentTag, out followingClan))
                {
                    followingClan = new FollowingClan { Tag = mismatch.OpponentTag };
                    clans.Add(mismatch.OpponentTag, followingClan);
                }

                followingClan.Name = mismatch.OpponentName;
                followingClan.BadgeURL = mismatch.OpponentBadgeUrl;
                followingClan.Wars++;
                followingClan.LatestTag = mismatch.ClanTag;
                followingClan.LatestClan = mismatch.ClanName;
                followingClan.LatestDate = mismatch.EndTime;
            }

            return View(clans.Values.Where(c => c.Wars > 1).OrderByDescending(c => c.LatestDate).ToList());
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
                    clanValidity.ValidFrom = opponent.MinEndTime.AddDays(-2);
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
        public async Task<IActionResult> Edit(string id, [Bind("Tag,Name,LinkID,Group,ValidFrom,ValidTo")] ClanValidity clanValidity)
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
                    //var opp = db.Wars.Where(o => o.OpponentTag == tag).OrderBy(o => o.EndTime).Select(o => o.OpponentName);
                    //if (opp.Count() > 0)
                    //{
                    //clanValidity.Name = opp.Last();

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

                    if (!clan.IsWarLogPublic)
                        clan.Wars = this.GetPrivateWars(tag);

                    if (clan.Wars != null)
                    {
                        var existingWars = db.Wars.Where(w => w.ClanTag == clan.Tag).ToDictionary(w => w.ID);
                        var clanValidities = db.ClanValidities.ToDictionary(v => v.Tag);
                        var syncs = db.WarSyncs.ToList();

                        foreach (var clanWar in clan.Wars)
                        {
                            if (clanWar.EndTime > clanValidity.ValidFrom && clanWar.EndTime < clanValidity.ValidTo)
                            {
                                if (clanValidities.ContainsKey(clanWar.OpponentTag))
                                {
                                    var opponentValidity = clanValidities[clanWar.OpponentTag];
                                    var searchTime = clanWar.SearchTime;
                                    if (opponentValidity.ValidFrom < searchTime && opponentValidity.ValidTo > searchTime)
                                        clanWar.Matched = true;
                                }

                                var warSyncs = (from s in syncs where s.Start >= clanWar.EndTime && s.Finish <= clanWar.EndTime select s).ToList();

                                foreach (var warSync in warSyncs)
                                {
                                    clanWar.Synced = true;
                                }

                                if (!existingWars.ContainsKey(clanWar.ID))
                                {
                                    db.Wars.Add(clanWar);
                                }
                                else
                                {
                                    var existingWar = existingWars[clanWar.ID];
                                    if(existingWar.Matched != clanWar.Matched || existingWar.Synced != clanWar.Synced)
                                    {
                                        existingWar.Matched = clanWar.Matched;
                                        existingWar.Synced = clanWar.Synced;
                                        db.Entry(existingWar).State = EntityState.Modified;
                                    }
                                }
                            }
                        }

                        foreach (var war in existingWars.Values)
                            if (war.EndTime > clanValidity.ValidTo || war.EndTime < clanValidity.ValidFrom)
                                db.Entry(war).State = EntityState.Deleted;
                    }

                    var opp = db.Wars.Where(o => o.OpponentTag == tag).OrderBy(o => o.EndTime);
                    foreach(var war in opp)
                    {
                        var searchTime = war.SearchTime;
                        var matched = false;
                        if(clanValidity.ValidFrom <= searchTime && clanValidity.ValidTo >= searchTime)
                        {
                            matched = true;
                        }
                        if(war.Matched != matched)
                        {
                            war.Matched = matched;
                            db.Entry(war).State = EntityState.Modified;
                        }
                    }
 
                    await db.SaveChangesAsync();
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

        public IActionResult Weight(string id)
        {
            var tag = LinkIdToTag(id);

            var clan = db.Clans.Where(c => c.Tag == tag).SingleOrDefault();

            var model = new WeightViewModel { ClanTag = clan.Tag, ClanLink = clan.LinkID, ClanName = clan.Name, ClanBadge = clan.BadgeUrl };

            var members = db.Members.Where(m => m.ClanTag == tag).OrderBy(m => m.ClanRank).ToList();

            var weights = (from m in db.Members join w in db.Weights on m.Tag equals w.Tag select w).ToDictionary(w => w.Tag);

            var memberWeights = new List<MemberWeightModel>();

            foreach(var member in members)
            {
                var memberWeight = new MemberWeightModel { Tag = member.Tag, Name = member.Name };
                Weight weight;
                if(weights.TryGetValue(member.Tag, out weight))
                {
                    memberWeight.InWar = weight.InWar;
                    memberWeight.Weight = weight.WarWeight;
                }
                memberWeights.Add(memberWeight);
            }

            model.Members = memberWeights.OrderByDescending(w => w.Weight).ToList();

            return View(model);
        }

        [HttpPost]
        public IActionResult Weight(string id, WeightViewModel model)
        {
            var tag = LinkIdToTag(id);

            foreach(var member in model.Members)
            {
                var weight = db.Weights.Where(w => w.Tag == member.Tag).SingleOrDefault();
                if (weight == null)
                {
                    weight = new Weight { Tag = member.Tag, WarWeight = member.Weight, InWar = member.InWar, LastModified = DateTime.UtcNow };
                    db.Add(weight);
                }
                else
                {
                    if (weight.WarWeight != member.Weight || weight.InWar != member.InWar)
                    {
                        weight.WarWeight = member.Weight;
                        weight.InWar = member.InWar;
                        weight.LastModified = DateTime.UtcNow;
                        db.Entry(weight).State = EntityState.Modified;
                    }
                }
            }

            db.SaveChanges();

            return Weight(id);
        }

    }
}
