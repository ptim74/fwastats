using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FWAStatsWeb.Data;
using FWAStatsWeb.Models;
using FWAStatsWeb.Models.ClanViewModels;
using FWAStatsWeb.Logic;
using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using Newtonsoft.Json;
using FWAStatsWeb.Services;
using Microsoft.AspNetCore.Identity;

namespace FWAStatsWeb.Controllers
{
    //[ResponseCache(Duration = Constants.CACHE_NORMAL)]
    public class ClansController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly ApplicationDbContext db;
        private readonly IClashApi api;
        private readonly ILogger<ClansController> logger;
        private readonly IGoogleSheetsService googleSheets;
        private readonly IClanLoader clanLoader;
        private readonly IOptions<WeightResultOptions> resultDatabase;
        private readonly WeightSubmitService submitService;

        public ClansController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext db,
            IClashApi api,
            ILogger<ClansController> logger,
            IGoogleSheetsService googleSheets,
            IClanLoader clanLoader,
            IOptions<WeightResultOptions> resultDatabase,
            WeightSubmitService submitService
            )
        {
            this.userManager = userManager;
            this.db = db;
            this.api = api;
            this.logger = logger;
            this.googleSheets = googleSheets;
            this.clanLoader = clanLoader;
            this.resultDatabase = resultDatabase;
            this.submitService = submitService;
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
                Th14Count = c.Th14Count,
                Th13Count = c.Th13Count,
                Th12Count = c.Th12Count,
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

            var model =  GetClanList();

            return View(model);
        }

        public ActionResult Departed()
        {
            logger.LogInformation("Departed");

            var clans = new List<FormerClan>();

            var validTo = DateTime.UtcNow.AddMonths(-1); // 1 month delay requested by fwa admins

            var clanQ = from c in db.ClanValidities where c.ValidTo < validTo orderby c.ValidTo descending select c;

            var clanBadges = (from w in db.Wars group w by w.OpponentTag into g select new { Tag = g.Key, BadgeUrl = g.Max(w => w.OpponentBadgeUrl) }).ToDictionary(w => w.Tag, w => w.BadgeUrl);

            foreach (var clan in clanQ)
            {
                var clanDetail = new FormerClan
                {
                    Tag = clan.Tag,
                    Name = clan.Name,
                    Group = clan.Group,
                    ValidFrom = clan.ValidFrom,
                    ValidTo = clan.ValidTo
                };

                if (clanBadges.TryGetValue(clan.Tag, out string badgeUrl))
                    clanDetail.BadgeURL = badgeUrl;

                clans.Add(clanDetail);
            }

            return View(clans);
        }

        protected List<War> GetPrivateWars(string id)
        {
            var wars = new List<War>();
            var opponentsWars = db.Wars.Where(o => o.OpponentTag == id && o.PreparationStartTime < Constants.MaxVisibleSearchTime).OrderByDescending(w => w.EndTime).ToList();
            if (opponentsWars.Count > 0)
            {
                foreach (var o in opponentsWars)
                {
                    var w = new War
                    {
                        ID = o.ID.Replace(o.ClanTag, o.OpponentTag),
                        EndTime = o.EndTime,
                        PreparationStartTime = o.PreparationStartTime,
                        StartTime = o.StartTime,
                        TeamSize = o.TeamSize,
                        Matched = o.Matched,
                        Synced = o.Synced,
                        ClanTag = o.OpponentTag,
                        ClanAttacks = 0,
                        ClanBadgeUrl = o.OpponentBadgeUrl,
                        ClanDestructionPercentage = o.OpponentDestructionPercentage,
                        ClanExpEarned = 0,
                        ClanLevel = o.OpponentLevel,
                        ClanName = o.OpponentName,
                        ClanStars = o.OpponentStars,
                        OpponentBadgeUrl = o.ClanBadgeUrl,
                        OpponentDestructionPercentage = o.ClanDestructionPercentage,
                        OpponentLevel = o.ClanLevel,
                        OpponentName = o.ClanName,
                        OpponentStars = o.ClanStars,
                        OpponentTag = o.ClanTag
                    };

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
            var clan = new Clan
            {
                Tag = tag,
                Name = "Clan not found",
                MemberList = new List<Member>()
            };

            try
            {
                var clans = db.Clans.Where(c => c.Tag == tag).ToList();
                if (clans.Count > 0)
                {
                    clan = clans.First();

                    clan.MemberList = db.Members.Where(m => m.ClanTag == clan.Tag).OrderBy(m => m.ClanRank).ToList();

                    if (clan.IsWarLogPublic)
                        clan.Wars = db.Wars.Where(w => w.ClanTag == tag && w.PreparationStartTime < Constants.MaxVisibleSearchTime).OrderByDescending(w => w.EndTime).ToList();
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
                                if(!warLookup.TryGetValue(warKey, out War clanWar))
                                {
                                    var synced = syncTimes.Where(s => s.Start <= war.PreparationStartTime && s.Finish >= war.PreparationStartTime).FirstOrDefault();
                                    if (synced != null && (war.TeamSize == Constants.WAR_SIZE1 || war.TeamSize == Constants.WAR_SIZE2))
                                        war.Synced = true;

                                    wars.Add(war);
                                    clanWar = war;
                                }

                                var matched = validities.Where(v => v.Tag == war.OpponentTag && v.ValidFrom <= war.PreparationStartTime && v.ValidTo >= war.PreparationStartTime).FirstOrDefault();
                                if (matched != null)
                                    clanWar.Matched = true;
                            }
                            clan.Wars = wars.OrderByDescending(w => w.PreparationStartTime).ToList();
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

            var details = new DetailsViewModel
            {
                InAlliance = db.Clans.Any(c => c.Tag == tag),
                Clan = await this.GetDetails(tag),
                Validity = this.db.ClanValidities.SingleOrDefault(c => c.Tag == tag),
                Events = new List<ClanDetailsEvent>()
            };

            var blacklisted = db.BlacklistedClans.Select(c => c.Tag).ToList();

            if (details.Clan.Wars != null)
            {
                foreach (var war in details.Clan.Wars)
                {
                    if (blacklisted.Contains(war.OpponentTag))
                        war.Blacklisted = true;
                }
            }

            details.Clan.Th14Count = 0;
            details.Clan.Th13Count = 0;
            details.Clan.Th12Count = 0;
            details.Clan.Th11Count = 0;
            details.Clan.Th10Count = 0;
            details.Clan.Th9Count = 0;
            details.Clan.Th8Count = 0;
            details.Clan.ThLowCount = 0;

            var thlevels = (from p in db.Players join m in db.Members on p.Tag equals m.Tag where m.ClanTag == tag select new { p.Tag, p.TownHallLevel }).ToList();
            foreach (var thlevel in thlevels)
            {
                var member = details.Clan.MemberList.SingleOrDefault(m => m.Tag == thlevel.Tag);
                if (member != null)
                {
                    member.TownHallLevel = thlevel.TownHallLevel;
                    if (member.TownHallLevel == 14)
                        details.Clan.Th14Count++;
                    else if (member.TownHallLevel == 13)
                        details.Clan.Th13Count++;
                    else if (member.TownHallLevel == 12)
                        details.Clan.Th12Count++;
                    else if (member.TownHallLevel == 11)
                        details.Clan.Th11Count++;
                    else if (member.TownHallLevel == 10)
                        details.Clan.Th10Count++;
                    else if (member.TownHallLevel == 9)
                        details.Clan.Th9Count++;
                    else if (member.TownHallLevel == 8)
                        details.Clan.Th8Count++;
                    else
                        details.Clan.ThLowCount++;
                }
            }

            if (details.Clan.Members == Constants.WAR_SIZE2)
            {
                var weights = (from w in db.Weights join m in db.Members on w.Tag equals m.Tag where m.ClanTag == tag select new { w.Tag, w.WarWeight }).ToList();

                details.Clan.EstimatedWeight = 0;

                foreach (var member in details.Clan.MemberList)
                {
                    var weight = weights.SingleOrDefault(w => w.Tag == member.Tag);
                    if (weight != null && weight.WarWeight > 0)
                    {
                        details.Clan.EstimatedWeight += weight.WarWeight / 1000;
                    }
                    else
                    {
                        if (member.TownHallLevel == 13)
                            details.Clan.EstimatedWeight += 125;
                        else if (member.TownHallLevel == 12)
                            details.Clan.EstimatedWeight += 115;
                        else if (member.TownHallLevel == 11)
                            details.Clan.EstimatedWeight += 105;
                        else if (member.TownHallLevel == 10)
                            details.Clan.EstimatedWeight += 85;
                        else if (member.TownHallLevel == 9)
                            details.Clan.EstimatedWeight += 65;
                        else if (member.TownHallLevel == 8)
                            details.Clan.EstimatedWeight += 50;
                        else if (member.TownHallLevel == 7)
                            details.Clan.EstimatedWeight += 35;
                        else if (member.TownHallLevel == 6)
                            details.Clan.EstimatedWeight += 25;
                        else if (member.TownHallLevel == 5)
                            details.Clan.EstimatedWeight += 15;
                        else if (member.TownHallLevel == 4)
                            details.Clan.EstimatedWeight += 7;
                        else if (member.TownHallLevel == 3)
                            details.Clan.EstimatedWeight += 3;
                        else if (member.TownHallLevel == 2)
                            details.Clan.EstimatedWeight += 1;
                    }

                }
            }

            var clanEvents = from e in db.PlayerEvents
                             join p in db.Players on e.PlayerTag equals p.Tag
                             where e.ClanTag == tag
                             && e.EventType != PlayerEventType.Stars
                             orderby e.EventDate descending
                             select new { Event = e, p.Name };

            foreach (var clanEvent in clanEvents.Take(100))
            {
                var e = new ClanDetailsEvent { Tag = clanEvent.Event.PlayerTag, Name = clanEvent.Name, EventDate = clanEvent.Event.EventDate, EventType = clanEvent.Event.EventType, TimeDesc = clanEvent.Event.TimeDesc() };
                if (e.EventType == PlayerEventType.Promote || e.EventType == PlayerEventType.Demote)
                {
                    e.Value = clanEvent.Event.RoleName;
                }
                else if(e.EventType == PlayerEventType.NameChange)
                {
                    e.Value = clanEvent.Event.StringValue;
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

            return View(details);
        }

        public ActionResult Following()
        {
            logger.LogInformation("Following");

            var clans = new Dictionary<string, FollowingClan>();

            var blacklisted = db.BlacklistedClans.Select(c => c.Tag).ToDictionary(c => c);

            var mismatches = from w in db.Wars where w.Synced == true && w.Matched == false && w.PreparationStartTime < Constants.MaxVisibleSearchTime orderby w.ID select w;

            foreach (var mismatch in mismatches)
            {
                if (!clans.TryGetValue(mismatch.OpponentTag, out FollowingClan followingClan))
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
                followingClan.LatestDate = mismatch.PreparationStartTime;
            }

            var model = clans.Values.Where(c => c.Wars > 1).OrderByDescending(c => c.LatestDate).ToList();

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
            else
            {
                throw new Exception("War not found.");
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

                    foreach(var war in db.Wars.Where( w => w.ClanTag == tag || w.OpponentTag == w.ClanTag))
                    {
                        if(war.PreparationStartTime < clanValidity.ValidFrom || war.PreparationStartTime > clanValidity.ValidTo)
                        {
                            war.Synced = false; // this is fixed in next sync
                            war.Matched = false;
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
                return RedirectToAction("Details", new { id });
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

            var result = db.WeightResults.SingleOrDefault(r => r.Tag == tag);
            if(result != null)
            {
                model.WeightSubmitDate = result.Timestamp;
                model.PendingWeightSubmit = result.PendingResult;
            }

            model.Wars = new List<WeightWarModel>();

            foreach (var war1 in this.GetDetailedWars(tag).Reverse())
            {
                model.Wars.Add(new WeightWarModel { ID = Utils.WarTimeToId(war1.EndTime), OpponentName = war1.OpponentName });
            }

            var war = db.Wars.Where(w => w.ClanTag == tag && w.EndTime == warEndTime).SingleOrDefault();

            if (war == null) //All clan members
            {
                if(model.Wars != null && model.Wars.Count > 0 )
                {
                    model.OpponentName = model.Wars.First().OpponentName;
                }

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
                    if (thlevels.TryGetValue(member.Tag, out int thlevel))
                        memberWeight.TownHallLevel = thlevel;
                    memberWeights.Add(memberWeight);
                }

                model.Members = memberWeights.OrderByDescending(w => w.Weight + w.TownHallLevel).ToList();

            }
            else //clan members of war
            {
                model.OpponentTag = war.OpponentTag;
                model.OpponentName = war.OpponentName;

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

            var clanWeight = 0;
            var memberCount = 0;
            var thCount = 0;
            var comparisons = new Dictionary<int, WeightComparison>();
            foreach (var member in model.Members.OrderByDescending(m => m.Weight))
            {
                if (member.Weight > 0 && member.InWar == true)
                {
                    memberCount++;
                    clanWeight += member.Weight;
                    thCount += member.TownHallLevel;
                    comparisons.Add(memberCount, new WeightComparison { Position = memberCount, Weight = member.Weight, Max = int.MinValue, Min = int.MaxValue });
                }
            }

            if(memberCount == Constants.WAR_SIZE1 || memberCount == Constants.WAR_SIZE2)
            {
                var maxWeight = clanWeight + Constants.WEIGHT_COMPARE; //30000
                var minWeight = clanWeight - Constants.WEIGHT_COMPARE;
                var results = db.WeightResults.Where(w => w.Weight >= minWeight && w.Weight <= maxWeight && w.TeamSize == memberCount && w.Tag != tag).ToList();

                model.ComparisonSampleSize = results.Count;
                //double sumOfSquares = 0D;
                double sumOfAbs = 0D;

                if (results.Count > 0)
                {
                    foreach (var res in results)
                    {
                        for (var i = 1; i <= memberCount; i++)
                        {
                            if (comparisons.TryGetValue(i, out WeightComparison comparison))
                            {
                                var weight = res.GetBase(i);
                                comparison.Average += weight;
                                if (weight > comparison.Max)
                                    comparison.Max = weight;
                                if (weight < comparison.Min)
                                    comparison.Min = weight;
                            }
                        }
                    }

                    model.Comparisons = new List<WeightComparison>();

                    for (var i = 1; i <= memberCount; i++)
                    {
                        if (comparisons.TryGetValue(i, out WeightComparison comparison))
                        {
                            comparison.Average /= results.Count;
                            comparison.Average /= 1000;
                            comparison.Average = Math.Round(comparison.Average, 1);
                            comparison.Weight /= 1000;
                            comparison.Min /= 1000;
                            comparison.Max /= 1000;
                            model.Comparisons.Add(comparison);
                            var diff = comparison.Weight - comparison.Average;
                            //sumOfSquares += diff * diff;
                            sumOfAbs += Math.Abs(diff);
                        }
                    }
                }

                //model.ComparisonDeviation =  (int)(Math.Sqrt(sumOfSquares) / memberCount * 1000);
                model.ComparisonDeviation = (int)(sumOfAbs / memberCount * 1000);
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

        [Route("Clan/{id}/WeightForm")]
        [Route("Clan/{id}/WeightForm/{WarID}")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult WeightForm(string id, long WarID)
        {
            logger.LogInformation("WeightForm {0}", id);

            var model = WeightData(id, WarID);

            if (model.Members.Count != Constants.WAR_SIZE1 && model.Members.Count != Constants.WAR_SIZE2)
                throw new Exception("Select 40 or 50 members");

            var form_id = "1FAIpQLSfckkdP851e2Ri_nHtO0XEWe2u3o0aA4NbJrMub06-zRkCzTQ";
            if (model.Members.Count == Constants.WAR_SIZE1)
                form_id = "1FAIpQLSfYtzGV7C7VHWQs0Z7r0aAOTtNR-9IbraAyvAEaNApIQNsiQQ";

            var url = string.Format("https://docs.google.com/forms/d/e/{0}/viewform?usp=pp_url", form_id);

            var ids = new List<int>
            {
                1984567353,
                1690011935,
                403343332,
                370160221,
                1607084216,
                1882413334,
                1543939269,
                685739330,
                463435811,
                898718070,
                1948329846,
                383495722,
                2020682725,
                331529930,
                1050253465,
                819675385,
                760895081,
                244948353,
                1506790920,
                915825883,
                258936521,
                742699705,
                1367988689,
                1491884247,
                167280690,
                1877792108,
                185518162,
                226536694,
                1109192970,
                888631196,
                1119555861,
                51377383,
                1284290519,
                500778845,
                61565236,
                895253717,
                1530094428,
                917968866,
                1120193979,
                38604784,
                985340327,
                1823279444,
                1688094775,
                189936802,
                1650758928,
                1048120044,
                1541727037,
                628819663,
                1485246803,
                612547503,
                1154234695,
                2118269445,
                1326597082,
                2128262510
            };

            int i = 0;

            url += string.Format("&entry.{0}={1}",ids[i++], Uri.EscapeDataString("'" + model.ClanName));
            url += string.Format("&entry.{0}={1}", ids[i++], "FWAStatsPrefilled");
            url += string.Format("&entry.{0}={1}", ids[i++], Uri.EscapeDataString(model.ClanTag));

            foreach(var m in model.Members)
                url += string.Format("&entry.{0}={1}", ids[i++], m.Weight);

            if (!string.IsNullOrEmpty(model.OpponentName))
            {
                if (model.Members.Count == Constants.WAR_SIZE1)
                    url += string.Format("&entry.{0}={1}", 790268828, Uri.EscapeDataString(model.OpponentName));
                else
                    url += string.Format("&entry.{0}={1}", ids[i++], Uri.EscapeDataString(model.OpponentName));
            }

            return Redirect(url);
        }

        [Route("Clan/{id}/WeightStatus")]
        [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult WeightStatus(string id)
        {
            var tag = Utils.LinkIdToTag(id);

            var model = new WeightStatusModel();

            var status = submitService.Status(tag);

            var runningTime = Convert.ToInt32(DateTime.UtcNow.Subtract(status.Timestamp).TotalSeconds);

            switch(status.Phase)
            {
                case SubmitPhase.Queued:
                    model.Final = false;
                    model.Result = false;
                    model.Text = $"Added to queue {runningTime} seconds ago.";
                    break;
                case SubmitPhase.Running:
                    model.Final = false;
                    model.Result = false;
                    model.Text = $"Processing started {runningTime} seconds ago. ({status.Message})";
                    break;
                case SubmitPhase.Succeeded:
                    model.Final = true;
                    model.Result = true;
                    model.Text = status.Message;
                    break;
                case SubmitPhase.Failed:
                    model.Final = true;
                    model.Result = false;
                    model.Text = status.Message;
                    break;
                default:
                    model.Final = true;
                    model.Result = false;
                    model.Text = "Unknown result";
                    break;
            }

            if (model.Final)
                logger.LogInformation("Weight.SubmitStatus {0}, {1}", id, model.Result);

            return Json(model);
        }

        protected async Task SaveWeight(WeightViewModel model)
        {
            if (model != null && model.Members != null)
            {
                var changes = 0;
                foreach (var member in model.Members)
                {
                    var weight = db.Weights.Where(w => w.Tag == member.Tag).SingleOrDefault();
                    if (weight == null)
                    {
                        changes++;
                        weight = new Weight { Tag = member.Tag, WarWeight = member.Weight, InWar = member.InWar, LastModified = DateTime.UtcNow };
                        db.Weights.Add(weight);
                    }
                    else
                    {
                        if (weight.WarWeight != member.Weight || weight.InWar != member.InWar)
                        {
                            if(weight.WarWeight != member.Weight)
                            {
                                changes++;
                            }
                            weight.WarWeight = member.Weight;
                            weight.InWar = member.InWar;
                            weight.LastModified = DateTime.UtcNow;
                        }
                    }
                }

                var ipAddr = GetIpAddr();
                var submitLog = new SubmitLog
                {
                    Tag = model.ClanTag,
                    Modified = DateTime.UtcNow,
                    IpAddr = ipAddr,
                    Changes = changes
                };
                db.SubmitLogs.Add(submitLog);

                await db.SaveChangesAsync();
            }
        }

        protected async Task QueueWeightSubmit(WeightViewModel weight)
        {
            var submit = new SubmitRequest
            {
                ClanTag = weight.ClanTag,
                ClanName = weight.ClanName,
                Mode = "submit",
                Members = new List<SubmitMember>()
            };

            //Read clan name from Proud FWA List
            var clans = await clanLoader.Load(Constants.LIST_FWA);
            if (clans != null)
            {
                var clan = clans.Where(c => c.Tag == weight.ClanTag).SingleOrDefault();
                if (clan != null)
                {
                    submit.ClanName = clan.Name;
                }
            }

            var position = 0;
            foreach(var m in weight.Members)
            {
                submit.Members.Add(new SubmitMember
                {
                    Position = ++position,
                    Tag = m.Tag,
                    Name = m.Name,
                    TownHall = m.TownHallLevel,
                    Weight = m.Weight
                });
            }

            submitService.Queue(submit);
        }

        [HttpPost]
        [Route("Clan/{id}/Weight")]
        public async Task<IActionResult> Weight(string id, WeightViewModel model)
        {
            logger.LogInformation("Weight.Post {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var clan = db.Clans.SingleOrDefault(c => c.Tag == tag);

            if(model.Command != null)
            {
                if (clan.SubmitRestriction != SubmitRestriction.Anyone)
                {
                    var userId = string.Empty;
                    var user = await GetCurrentUserAsync();
                    if (user != null)
                    {
                        logger.LogInformation("Weight.User {0}", user.Email);
                        userId = user.Id;
                    }
                    var players = from pc in db.PlayerClaims
                                  where pc.UserId == userId
                                  join m in db.Members on pc.Tag equals m.Tag
                                  where m.ClanTag == model.ClanTag
                                  select m;

                    var playerAccessLevel = 0;

                    foreach (var player in players)
                    {
                        if (player.Role == "member" && playerAccessLevel <= 0)
                            playerAccessLevel = 1;
                        if (player.Role == "admin" && playerAccessLevel <= 1)
                            playerAccessLevel = 2;
                        if (player.Role == "coLeader" && playerAccessLevel <= 2)
                            playerAccessLevel = 3;
                        if (player.Role == "leader" && playerAccessLevel <= 3)
                            playerAccessLevel = 4;
                    }
                    if (playerAccessLevel < 4 && clan.SubmitRestriction == SubmitRestriction.Leader)
                    {
                        ViewData["Message"] = "Only clan leader can edit weights.";
                        ViewData["ClanLink"] = clan.LinkID;
                        logger.LogWarning("Weight.AccessDenied LeaderRequired");
                        return View("WeightError");
                    }
                    if (playerAccessLevel < 3 && clan.SubmitRestriction == SubmitRestriction.CoLeaders)
                    {
                        ViewData["Message"] = "Only clan leader and co-leaders can edit weights.";
                        ViewData["ClanLink"] = clan.LinkID;
                        logger.LogWarning("Weight.AccessDenied CoLeaderRequired");
                        return View("WeightError");
                    }
                    if (playerAccessLevel < 2 && clan.SubmitRestriction == SubmitRestriction.Elders)
                    {
                        ViewData["Message"] = "Only clan leader, co-leaders and elders can edit weights.";
                        ViewData["ClanLink"] = clan.LinkID;
                        logger.LogWarning("Weight.AccessDenied ElderRequired");
                        return View("WeightError");
                    }
                    if (playerAccessLevel < 1 && clan.SubmitRestriction == SubmitRestriction.Members)
                    {
                        ViewData["Message"] = "Only clan members can edit weights.";
                        ViewData["ClanLink"] = clan.LinkID;
                        logger.LogWarning("Weight.AccessDenied MemberRequired");
                        return View("WeightError");
                    }
                }

                if (clan.SubmitRestriction == SubmitRestriction.Anyone)
                {
                    if (CheckSubmitChanges(tag, DateTime.UtcNow.AddHours(-2)) > 100)
                    {
                        ViewData["Message"] = "You have modified enough weights for a while. Please try again later.";
                        ViewData["ClanLink"] = clan.LinkID;
                        logger.LogWarning("Weight.AccessDenied MaxSubmitChanges");
                        return View("WeightError");
                    }
                }
                await SaveWeight(model);

                if (model.Command.Equals("submit", StringComparison.OrdinalIgnoreCase))
                {
                    var model2 = WeightData(id, model.WarID);
                    model2.WeightSubmitQueued = true;
                    await QueueWeightSubmit(model2);
                    return View(model2);
                }
            }

            return Weight(id, model.WarID);
        }

        protected string GetIpAddr()
        {
            var ipAddr = this.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (string.IsNullOrEmpty(ipAddr))
            {
                var originalFor = this.HttpContext.Request.Headers["X-Original-For"].FirstOrDefault();
                if (originalFor != null && originalFor.Contains(":"))
                    ipAddr = originalFor.Split(':').FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddr))
                ipAddr = "127.0.0.1";
            return ipAddr;
        }

        protected int CheckSubmitChanges(string tag, DateTime since)
        {
            var ipAddr = GetIpAddr();

            var changes = db.SubmitLogs
                .Where(l => l.IpAddr == ipAddr && l.Modified > since)
                .Select(l => l.Changes)
                .Sum();

            return changes;
        }

        [Route("Clan/{id}/Donations")]
        //[ResponseCache(Duration = Constants.CACHE_MIN)]
        public async Task<IActionResult> DonationData(string id)
        {
            var tag = Utils.LinkIdToTag(id);

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

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }

    }
}
