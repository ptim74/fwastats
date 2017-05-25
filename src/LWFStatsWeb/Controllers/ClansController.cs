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
using Microsoft.Extensions.Options;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Sheets.v4;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;

namespace LWFStatsWeb.Controllers
{
    public class ClansController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClashApi api;
        private IMemoryCache memoryCache;
        ILogger<ClansController> logger;
        IOptions<WeightSubmitOptions> submitOptions;

        public ClansController(
            ApplicationDbContext db,
            IClashApi api,
            IMemoryCache memoryCache,
            ILogger<ClansController> logger,
            IOptions<WeightSubmitOptions> submitOptions
            )
        {
            this.db = db;
            this.api = api;
            this.memoryCache = memoryCache;
            this.logger = logger;
            this.submitOptions = submitOptions;
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

            var model = memoryCache.GetOrCreate(Constants.CACHE_CLANS_ALL, entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);
                return GetClanList();
            });

            return View(model);
        }

        public ActionResult Departed()
        {
            logger.LogInformation("Departed");

            var model = memoryCache.GetOrCreate(Constants.CACHE_CLANS_DEPARTED, entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);

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
            var opponentsWars = db.Wars.Where(o => o.OpponentTag == id && o.EndTime < Constants.MaxVisibleEndTime).OrderByDescending(w => w.EndTime).ToList();
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
                        clan.Wars = db.Wars.Where(w => w.ClanTag == tag && w.EndTime < Constants.MaxVisibleEndTime).OrderByDescending(w => w.EndTime).ToList();
                    else
                        clan.Wars = this.GetPrivateWars(tag);
                }
                else
                {
                    var wars = this.GetPrivateWars(tag);
                    if (wars.Count > 0)
                    {
                        clan = await api.GetClan(tag, true, true);
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
                        clan = await api.GetClan(tag, true, true);
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

            var model = await memoryCache.GetOrCreateAsync(Constants.CACHE_CLANS_DETAILS_ + tag, async entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);

                var details = new DetailsViewModel();
                details.InAlliance = db.Clans.Any(c => c.Tag == tag);
                details.Clan = await this.GetDetails(tag);
                details.Validity = this.db.ClanValidities.SingleOrDefault(c => c.Tag == tag);
                details.Events = new List<ClanDetailsEvent>();

                var blacklisted = db.BlacklistedClans.Select(c => c.Tag).ToList();

                if (details.Clan.Wars != null)
                {
                    foreach (var war in details.Clan.Wars)
                    {
                        if (blacklisted.Contains(war.OpponentTag))
                            war.Blacklisted = true;
                    }
                }

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

                details.WarsWithDetails = new HashSet<string>();
                var warsWithMembers = (from w in db.Wars where w.ClanTag == tag join m in db.WarMembers on w.ID equals m.WarID select w.ID).Distinct().ToList();
                foreach (var warId in warsWithMembers)
                    details.WarsWithDetails.Add(warId);

                return details;
            });

            return View(model);
        }

        public ActionResult Following()
        {
            logger.LogInformation("Following");

            var model = memoryCache.GetOrCreate(Constants.CACHE_CLANS_FOLLOWING, entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);

                var clans = new Dictionary<string, FollowingClan>();

                var blacklisted = db.BlacklistedClans.Select(c => c.Tag).ToDictionary(c => c);

                var mismatches = from w in db.Wars where w.Synced == true && w.Matched == false && w.EndTime < Constants.MaxVisibleEndTime orderby w.ID select w;

                foreach (var mismatch in mismatches)
                {
                    FollowingClan followingClan = null;
                    if (!clans.TryGetValue(mismatch.OpponentTag, out followingClan))
                    {
                        followingClan = new FollowingClan { Tag = mismatch.OpponentTag };
                        if (blacklisted.ContainsKey(followingClan.Tag))
                        {
                            followingClan.Blacklisted = true;
                        }
                        clans.Add(mismatch.OpponentTag, followingClan);
                    }

                    followingClan.Name = mismatch.OpponentName;
                    followingClan.BadgeURL = mismatch.OpponentBadgeUrl;
                    followingClan.Wars++;
                    followingClan.LatestTag = mismatch.ClanTag;
                    followingClan.LatestClan = mismatch.ClanName;
                    followingClan.LatestDate = mismatch.SearchTime;
                }

                return clans.Values.Where(c => c.Wars > 1).OrderByDescending(c => c.LatestDate).ToList();
            });

            return View(model);
        }

        [Route("Clan/{id}/War/{warId}")]
        public async Task<IActionResult> WarDetails(string id, long warId)
        {
            logger.LogInformation("War {0} {1}", id, warId);

            var tag = Utils.LinkIdToTag(id);

            var endTime = Utils.WarIdToTime(warId);

            var war = await db.Wars.Where(w => w.ClanTag == tag && w.EndTime == endTime).SingleOrDefaultAsync();

            if(war != null)
            {
                war.Members = await db.WarMembers.Where(m => m.WarID == war.ID).OrderBy(m => m.Tag).ToListAsync();
                var attacks = await db.WarAttacks.Where(a => a.WarID == war.ID).OrderBy(a => a.Order).ToListAsync();

                var memberDict = war.Members.ToDictionary(m => m.Tag);

                foreach(var attack in attacks)
                {
                    if(memberDict.TryGetValue(attack.AttackerTag,out var member))
                    {
                        if (member.Attacks == null)
                            member.Attacks = new List<WarAttack>();
                        member.Attacks.Add(attack);
                    }
                }
            }

            return View(war);
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

                    var clan = await api.GetClan(tag, true, false);

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

                    memoryCache.Remove(Constants.CACHE_CLANS_DETAILS_ + tag);
                    memoryCache.Remove(Constants.CACHE_DATA_MEMBERS_ + tag);
                    memoryCache.Remove(Constants.CACHE_CLANS_ALL);
                    memoryCache.Remove(Constants.CACHE_CLANS_FOLLOWING);
                    memoryCache.Remove(Constants.CACHE_CLANS_DEPARTED);

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

        private ICollection<War> GetDetailedWars(string clanTag)
        {
            var minEndTime = DateTime.UtcNow.AddDays(-7);
            var maxEndTime = DateTime.UtcNow.AddDays(1);

            return db.Wars.Where(w => w.ClanTag == clanTag && w.EndTime > minEndTime && w.EndTime < maxEndTime).OrderBy(w => w.EndTime).ToList();
        }

        [Route("Clan/{id}/Attacks")]
        public IActionResult Attacks(string id)
        {
            logger.LogInformation("Attacks {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var clan = db.Clans.Where(c => c.Tag == tag).SingleOrDefault();

            var model = new ClanAttackModel
            {
                Tag = clan.Tag,
                Name = clan.Name,
                BadgeUrl = clan.BadgeUrl,
                Members = new List<ClanAttackMember>(),
                Wars = new List<ClanAttackWar>()
            };

            foreach(var member in db.Members.Where(m => m.ClanTag == tag).OrderBy(m => m.ClanRank).ToList())
            {
                model.Members.Add(new ClanAttackMember {
                     Name = member.Name,
                     Tag = member.Tag,
                     Attacks = new Dictionary<string, ICollection<WarAttack>>()
                });
            }

            var modelMemberDict = model.Members.ToDictionary(m => m.Tag);

            foreach (var war in this.GetDetailedWars(tag))
            {
                var warMembers = db.WarMembers.Where(m => m.WarID == war.ID && m.IsOpponent == false).OrderBy(m => m.Tag).ToDictionary(m => m.Tag);
                var warAttacks = db.WarAttacks.Where(a => a.WarID == war.ID && a.IsOpponent == false).OrderBy(a => a.Order).ToLookup(a => a.AttackerTag);

                foreach(var member in model.Members)
                {
                    if (warMembers.ContainsKey(member.Tag))
                    {
                        member.Attacks.Add(war.ID, warAttacks[member.Tag].ToList());
                    }
                }

                model.Wars.Add(new ClanAttackWar { ID = war.ID, OpponentName = war.OpponentName });
            }

            return View(model);
        }

        protected WeightViewModel WeightData(string id, long WarID)
        {
            var tag = Utils.LinkIdToTag(id);

            var clan = db.Clans.Where(c => c.Tag == tag).SingleOrDefault();

            var warEndTime = Utils.WarIdToTime(WarID);

            var model = new WeightViewModel { ClanTag = clan.Tag, ClanLink = clan.LinkID, ClanName = clan.Name, ClanBadge = clan.BadgeUrl, WarID = WarID };

            model.Wars = new List<WeightWarModel>();

            foreach (var war1 in this.GetDetailedWars(tag).Reverse())
            {
                model.Wars.Add(new WeightWarModel { ID = Utils.WarTimeToId(war1.EndTime), OpponentName = war1.OpponentName });
            }

            var war = db.Wars.Where(w => w.ClanTag == tag && w.EndTime == warEndTime).SingleOrDefault();

            if (war == null) //All clan members
            {
                var members = db.Members.Where(m => m.ClanTag == tag).OrderBy(m => m.ClanRank).ToList();

                var weights = (from m in db.Members where m.ClanTag == tag join w in db.Weights on m.Tag equals w.Tag select w).ToDictionary(w => w.Tag);

                var thlevels = (from p in db.Players join m in db.Members on p.Tag equals m.Tag where m.ClanTag == tag select new { p.Tag, p.TownHallLevel }).ToDictionary(p => p.Tag, t => t.TownHallLevel);

                var memberWeights = new List<MemberWeightModel>();

                foreach (var member in members)
                {
                    var memberWeight = new MemberWeightModel { Tag = member.Tag, Name = member.Name };
                    if (weights.TryGetValue(member.Tag, out Weight weight))
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

            }
            else //clan members of war
            {
                var members = db.WarMembers.Where(m => m.WarID == war.ID && m.IsOpponent == false).OrderBy(m => m.MapPosition).ToList();

                var weights = (from m in db.WarMembers join w in db.Weights on m.Tag equals w.Tag where m.WarID == war.ID select w).ToDictionary(w => w.Tag);

                var memberWeights = new List<MemberWeightModel>();

                foreach (var member in members)
                {
                    var memberWeight = new MemberWeightModel { Tag = member.Tag, Name = member.Name, TownHallLevel = member.TownHallLevel, InWar = true };
                    if (weights.TryGetValue(member.Tag, out Weight weight))
                    {
                        memberWeight.Weight = weight.WarWeight;
                    }
                    memberWeights.Add(memberWeight);
                }

                model.Members = memberWeights.ToList();
            }

            return model;
        }

        [Route("Clan/{id}/Weight")]
        [Route("Clan/{id}/Weight/{WarID}")]
        public IActionResult Weight(string id, long WarID)
        {
            logger.LogInformation("Weight {0}", id);

            var model = WeightData(id, WarID);

            return View(model);
        }

        protected void SaveWeight(WeightViewModel model)
        {
            if (model != null && model.Members != null)
            {
                foreach (var member in model.Members)
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
            }
        }

        [Route("Clan/{id}/WeightSubmitResult")]
        public IActionResult WeightSubmitResult(string id)
        {
            var service = CreateSheetService();

            var check = service.Spreadsheets.Values.Get(submitOptions.Value.SheetId, submitOptions.Value.ResultRange);
            check.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS;
            check.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
            var result = check.Execute();

            var resultText = string.Empty;

            if (result.Values != null)
                foreach (var i in result.Values)
                    foreach (var j in i)
                        resultText = j.ToString();

            return Json(resultText);
        }

        protected IActionResult WeightSubmit(WeightViewModel weight)
        {
            var model = new WeightSubmitModel()
            {
                ClanTag = weight.ClanTag,
                ClanName = weight.ClanName,
                ClanLink = weight.ClanLink,
                ClanBadge = weight.ClanBadge, 
                SheetUrl = "http://tinyurl.com/FWABaseWeightResponse"
            };

            try
            {
                logger.LogInformation("Weight.Submit {0}", weight.ClanLink);

                var service = CreateSheetService();

                var lastSubmitDate = DateTime.MinValue;

                var check = service.Spreadsheets.Values.Get(submitOptions.Value.SheetId, submitOptions.Value.CheckRange);
                check.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS;
                check.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
                var result = check.Execute();

                if (result.Values != null)
                    foreach(var i in result.Values)
                        foreach (var j in i)
                            DateTime.TryParse(j.ToString(), out lastSubmitDate);

                if (lastSubmitDate > DateTime.UtcNow.AddMinutes(-1))
                    throw new Exception("Someone else is using weight sheet right now, please try again later");

                var nameSection = new List<object>();
                nameSection.Add(weight.ClanName);
                nameSection.Add("");
                nameSection.Add("");
                nameSection.Add(weight.ClanTag);

                var compositions = new Dictionary<int, int>();
                var weightSection = new List<object>();
                var tagSection = new List<object>();
                var thSection = new List<object>();

                for (int i = 0; i <= 11; i++)
                    compositions.Add(i, 0);

                foreach(var member in weight.Members)
                {
                    compositions[member.TownHallLevel]++;
                    weightSection.Add(member.Weight);
                    tagSection.Add(member.Tag);
                    thSection.Add(member.TownHallLevel);
                }

                var compositionSection = new List<object>();
                compositionSection.Add(compositions[11]);
                compositionSection.Add(compositions[10]);
                compositionSection.Add(compositions[9]);
                compositionSection.Add(compositions[8]);
                compositionSection.Add(compositions[7] + compositions[6] + compositions[5] + compositions[4] + compositions[3]);

                var submitSection = new List<object>();
                submitSection.Add("SubmitDataAuto");

                var updateRequestBody = new BatchUpdateValuesRequest();
                updateRequestBody.Data = new List<ValueRange>();

                updateRequestBody.Data.Add(new ValueRange { MajorDimension = "COLUMNS", Range = submitOptions.Value.ClanNameRange, Values = new List<IList<object>> { nameSection } });

                updateRequestBody.Data.Add(new ValueRange { MajorDimension = "COLUMNS", Range = submitOptions.Value.CompositionRange, Values = new List<IList<object>> { compositionSection } });

                updateRequestBody.Data.Add(new ValueRange { MajorDimension = "COLUMNS", Range = submitOptions.Value.WeightRange, Values = new List<IList<object>> { weightSection } });

                updateRequestBody.Data.Add(new ValueRange { MajorDimension = "COLUMNS", Range = submitOptions.Value.TagRange, Values = new List<IList<object>> { tagSection } });

                updateRequestBody.Data.Add(new ValueRange { MajorDimension = "COLUMNS", Range = submitOptions.Value.THRange, Values = new List<IList<object>> { thSection } });

                updateRequestBody.Data.Add(new ValueRange { MajorDimension = "COLUMNS", Range = submitOptions.Value.AutoSubmitRange, Values = new List<IList<object>> { submitSection } });

                updateRequestBody.Data.Add(new ValueRange { MajorDimension = "COLUMNS", Range = submitOptions.Value.CheckRange, Values = new List<IList<object>> { new List<object> { DateTime.UtcNow.ToString("s") } } });

                updateRequestBody.ValueInputOption = "RAW";
                updateRequestBody.IncludeValuesInResponse = false;

                var updateRequest = service.Spreadsheets.Values.BatchUpdate(updateRequestBody, submitOptions.Value.SheetId);

                var updateResponse = updateRequest.Execute();

                model.Message = "Redirecting to Weight Sheet...";
                model.Status = true;
            }
            catch(Exception e)
            {
                model.Message = e.Message;
            }
            return View("WeightSubmit", model);
        }

        private SheetsService CreateSheetService()
        {
            return new SheetsService(
                new BaseClientService.Initializer()
                {
                    ApplicationName = "FWA Stats",
                    HttpClientInitializer = new ServiceAccountCredential(
                        new ServiceAccountCredential.Initializer(submitOptions.Value.ClientEmail)
                        {
                            Scopes = new[] { SheetsService.Scope.Spreadsheets }
                        }.FromPrivateKey(submitOptions.Value.PrivateKey))
                });
        }

        [HttpPost]
        [Route("Clan/{id}/Weight")]
        public IActionResult Weight(string id, WeightViewModel model)
        {
            logger.LogInformation("Weight.Post {0}", id);

            var tag = Utils.LinkIdToTag(id);

            if(model.Command != null)
            {
                SaveWeight(model);
                memoryCache.Remove(Constants.CACHE_DATA_MEMBERS_ + tag);

                if ( model.Command.Equals("submit", StringComparison.OrdinalIgnoreCase))
                {
                    return WeightSubmit(WeightData(id,model.WarID));
                }
            }

            return Weight(id, model.WarID);
        }

        [Route("Clan/{id}/Donations")]
        public async Task<IActionResult> DonationData(string id, int counter)
        {
            var tag = Utils.LinkIdToTag(id);

            if (counter == 0)
            {
                logger.LogInformation("Tracking {0} Started", id);
            }
            else if (counter > 1440)
            {
                logger.LogInformation("Tracking {0} Stopped", id);
                return NoContent();
            }
            else if(counter < 30 || counter % 10 == 0)
            {
                logger.LogInformation("Tracking {0} #{1}", id, counter);
            }

            var data = new List<DonationTrackModel>();

            var clan = await api.GetClan(tag, false, false);

            if (clan.MemberList != null)
            {
                foreach (var member in clan.MemberList)
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

            return Json(data);
        }

        [Route("Clan/{id}/Track")]
        public IActionResult Track(string id)
        {
            logger.LogInformation("Track {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var clan = db.Clans.SingleOrDefault(c => c.Tag == tag);
            if (clan == null)
                return NotFound();

            return View(clan);
        }

    }
}
