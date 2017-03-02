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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace LWFStatsWeb.Controllers
{
    public class ClansController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClashApi api;
        private IMemoryCache memoryCache;
        ILogger<ClansController> logger;

        public ClansController(
            ApplicationDbContext db,
            IClashApi api,
            IMemoryCache memoryCache,
            ILogger<ClansController> logger)
        {
            this.db = db;
            this.api = api;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        protected IndexViewModel GetClanList()
        {
            var clans = new IndexViewModel();

            var clanQ = db.Clans.Select(c => new ClanIndexClan
            {
                Tag = c.Tag,
                Name = c.Name,
                Members = c.Members,
                BadgeUrl = c.BadgeUrl,
                Th11Count = c.Th11Count,
                Th10Count = c.Th10Count,
                Th9Count = c.Th9Count,
                Th8Count = c.Th8Count,
                ThLowCount = c.ThLowCount,
                WarCount = c.WarCount,
                MatchPercentage = c.MatchPercentage,
                WinPercentage = c.WinPercentage,
                EstimatedWeight = c.EstimatedWeight
            });

            foreach (var clan in clanQ.OrderBy(c => c.Name.ToLower()))
            {
                clans.Add(clan);
            }

            return clans;
        }

        // GET: Clans
        public ActionResult Index()
        {
            logger.LogInformation("Index");

            var model = memoryCache.GetOrCreate("Clans.All", entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return GetClanList();
            });

            return View(model);
        }

        public ActionResult Departed()
        {
            logger.LogInformation("Departed");

            var model = memoryCache.GetOrCreate("Clans.Departed", entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                var clans = new List<FormerClan>();

                var clanQ = from c in db.ClanValidities where c.ValidTo < DateTime.Now orderby c.ValidTo descending select c;

                var clanBadges = (from w in db.Wars group w by w.OpponentTag into g select new { Tag = g.Key, BadgeUrl = g.Max(w => w.OpponentBadgeUrl) }).ToDictionary(w => w.Tag, w => w.BadgeUrl);

                foreach (var clan in clanQ)
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

                return clans;
            });

            return View(model);
        }

        protected List<War> GetPrivateWars(string id)
        {
            var wars = new List<War>();
            var opponentsWars = db.Wars.Where(o => o.OpponentTag == id).OrderByDescending(w => w.EndTime).ToList();
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
                var clans = db.Clans.Where(c => c.Tag == tag).ToList();
                if (clans.Count > 0)
                {
                    clan = clans.First();

                    clan.MemberList = db.Members.Where(m => m.ClanTag == clan.Tag).OrderBy(m => m.ClanRank).ToList();

                    if (clan.IsWarLogPublic)
                        clan.Wars = db.Wars.Where(c => c.ClanTag == tag).OrderByDescending(w => w.EndTime).ToList();
                    else
                        clan.Wars = this.GetPrivateWars(tag);
                }
                else
                {
                    var wars = this.GetPrivateWars(tag);
                    if (wars.Count > 0)
                    {
                        clan = await api.GetClan(tag, true);
                        if (clan.Wars != null && clan.Wars.Count > 0)
                        {
                            var syncTimes = db.WarSyncs.Select(s => new { s.Start, s.Finish }).ToArray();
                            var validities = db.ClanValidities.ToList();
                            var warLookup = wars.ToDictionary(w => string.Format("{0}{1}", w.OpponentTag, w.EndTime.Date));
                            foreach (var war in clan.Wars)
                            {
                                var warKey = string.Format("{0}{1}", war.OpponentTag, war.EndTime.Date);
                                War clanWar = null;
                                if(!warLookup.TryGetValue(warKey, out clanWar))
                                {
                                    var synced = syncTimes.Where(s => s.Start <= war.EndTime && s.Finish >= war.EndTime).FirstOrDefault();
                                    if (synced != null && war.TeamSize == 40)
                                        war.Synced = true;

                                    wars.Add(war);
                                    clanWar = war;
                                }

                                var matched = validities.Where(v => v.Tag == war.OpponentTag && v.ValidFrom <= war.SearchTime && v.ValidTo >= war.EndTime).FirstOrDefault();
                                if (matched != null)
                                    clanWar.Matched = true;
                            }
                            clan.Wars = wars.OrderByDescending(w => w.EndTime).ToList();
                        }
                        else
                        {
                            clan.Wars = wars;
                        }
                    }
                    else
                    {
                        clan = await api.GetClan(tag, true);
                    }
                    
                }
            }
            catch (Exception e)
            {
                clan.Description = e.Message;
            }
            return clan;
        }

        [Route("Clan/{id}")]
        public async Task<ActionResult> Details(string id)
        {
            logger.LogInformation("Details {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var model = await memoryCache.GetOrCreateAsync("ClanDetails." + tag, async entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                var details = new DetailsViewModel();
                details.InAlliance = db.Clans.Any(c => c.Tag == tag);
                details.Clan = await this.GetDetails(tag);
                details.Validity = this.db.ClanValidities.SingleOrDefault(c => c.Tag == tag);
                details.Events = new List<ClanDetailsEvent>();

                var thlevels = (from p in db.Players join m in db.Members on p.Tag equals m.Tag where m.ClanTag == tag select new { p.Tag, p.TownHallLevel });
                foreach(var thlevel in thlevels.ToList())
                {
                    var member = details.Clan.MemberList.SingleOrDefault(m => m.Tag == thlevel.Tag);
                    if(member != null)
                        member.TownHallLevel = thlevel.TownHallLevel;
                }

                var clanEvents = from e in db.PlayerEvents
                                join p in db.Players on e.PlayerTag equals p.Tag
                                where e.ClanTag == tag
                                && e.EventType != PlayerEventType.Stars
                                orderby e.EventDate descending
                                select new { Event = e, Name = p.Name };

                foreach(var clanEvent in clanEvents.Take(100))
                {
                    var e = new ClanDetailsEvent { Tag = clanEvent.Event.PlayerTag, Name = clanEvent.Name, EventDate = clanEvent.Event.EventDate, EventType = clanEvent.Event.EventType, TimeDesc = clanEvent.Event.TimeDesc() };
                    if(e.EventType == PlayerEventType.Promote || e.EventType == PlayerEventType.Demote)
                    {
                        e.Value = clanEvent.Event.RoleName;
                    }
                    else
                    {
                        e.Value = clanEvent.Event.Value.ToString();
                    }
                    details.Events.Add(e);
                }

                return details;
            });

            return View(model);
        }

        public ActionResult Following()
        {
            logger.LogInformation("Following");

            var model = memoryCache.GetOrCreate("Clans.Following", entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                var clans = new Dictionary<string, FollowingClan>();

                var mismatches = from w in db.Wars where w.Synced == true && w.Matched == false orderby w.ID select w;

                foreach (var mismatch in mismatches)
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

                return clans.Values.Where(c => c.Wars > 1).OrderByDescending(c => c.LatestDate).ToList();
            });

            return View(model);
        }

        [Route("Clan/{id}/Edit")]
        public async Task<IActionResult> Edit(string id)
        {
            logger.LogInformation("Edit {0}", id);

            var tag = Utils.LinkIdToTag(id);
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
        [Route("Clan/{id}/Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Tag,Name,LinkID,Group,ValidFrom,ValidTo")] ClanValidity clanValidity)
        {
            logger.LogInformation("Edit.Post {0}", id);

            var tag = Utils.LinkIdToTag(id);

            if (tag != clanValidity.Tag)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {

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

                    memoryCache.Remove("ClanDetails." + tag);
                    memoryCache.Remove("ClanMembers." + tag);
                    memoryCache.Remove("Clans.All");
                    memoryCache.Remove("Clans.FWA");
                    memoryCache.Remove("Clans.FWAL");
                    memoryCache.Remove("Clans.Following");
                    memoryCache.Remove("Clans.Departed");

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

        [Route("Clan/{id}/Weight")]
        public IActionResult Weight(string id)
        {
            logger.LogInformation("Weight {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var clan = db.Clans.Where(c => c.Tag == tag).SingleOrDefault();

            var model = new WeightViewModel { ClanTag = clan.Tag, ClanLink = clan.LinkID, ClanName = clan.Name, ClanBadge = clan.BadgeUrl };

            var members = db.Members.Where(m => m.ClanTag == tag).OrderBy(m => m.ClanRank).ToList();

            var weights = (from m in db.Members join w in db.Weights on m.Tag equals w.Tag select w).ToDictionary(w => w.Tag);

            var thlevels = (from p in db.Players join m in db.Members on p.Tag equals m.Tag where m.ClanTag == tag select new { p.Tag, p.TownHallLevel }).ToDictionary(p => p.Tag, t => t.TownHallLevel);

            //var wars = (from w in db.Wars where w.ClanTag == tag && w.Synced orderby w.EndTime descending).Take(3).Select()

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
                int thlevel = 0;
                if (thlevels.TryGetValue(member.Tag, out thlevel))
                    memberWeight.TownHallLevel = thlevel;
                memberWeights.Add(memberWeight);
            }

            model.Members = memberWeights.OrderByDescending(w => w.Weight + w.TownHallLevel).ToList();

            return View(model);
        }

        [HttpPost]
        [Route("Clan/{id}/Weight")]
        public IActionResult Weight(string id, WeightViewModel model)
        {
            logger.LogInformation("Weight.Post {0}", id);

            var tag = Utils.LinkIdToTag(id);

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

            memoryCache.Remove("ClanMembers." + tag);

            return Weight(id);
        }

        [Route("Clan/{id}/Donations")]
        public async Task<IActionResult> DonationData(string id, int counter)
        {
            var tag = Utils.LinkIdToTag(id);

            if(counter == 0)
            {
                logger.LogInformation("Tracking {0} Started", id);
            }

            if (counter > 240)
            {
                logger.LogInformation("Tracking {0} Stopped", id);
                return NoContent();
            }

            var model = await memoryCache.GetOrCreate(String.Format("ClanDonations.", tag), async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(50);

                var data = new List<DonationTrackModel>();

                var clan = await api.GetClan(tag, false);

                if(clan.MemberList != null)
                {
                    foreach(var member in clan.MemberList)
                    {
                        data.Add(new DonationTrackModel
                        {
                            Tag = member.Tag,
                            Name = member.Name,
                            Donated = member.Donations,
                            Received = member.DonationsReceived
                        });
                    }
                }

                return data;
            });

            return Json(model);
        }

        [Route("Clan/{id}/Track")]
        public IActionResult Track(string id)
        {
            logger.LogInformation("Track {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var clan = db.Clans.SingleOrDefault(c => c.Tag == tag);

            return View(clan);
        }

    }
}
