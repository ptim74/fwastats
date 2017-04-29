using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Models;
using LWFStatsWeb.Models.UpdateViewModels;
using LWFStatsWeb.Data;
using LWFStatsWeb.Logic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LWFStatsWeb.Controllers
{
    public class UpdateController : Controller
    {
        private static object lockObject = new object();
        private readonly ApplicationDbContext db;
        private readonly IClanLoader loader;
        private readonly IClanUpdater updater;
        private readonly IClashApi api;
        private readonly IClanStatistics statistics;
        ILogger<UpdateController> logger;

        public UpdateController(
            ApplicationDbContext context, 
            IClanLoader loader,
            IClanUpdater updater,
            IClashApi api,
            IClanStatistics statistics,
            ILogger<UpdateController> logger)
        {
            this.db = context;
            this.loader = loader;
            this.updater = updater;
            this.api = api;
            this.statistics = statistics;
            this.logger = logger;
        }

        protected async Task<IndexViewModel> GetUpdates()
        {
            var model = new IndexViewModel();

            var loadedClans = await loader.Load();

            model.Errors = loader.Errors;

            if (loadedClans.Count > 0)
            {
                model.Tasks = updater.GetUpdates(loadedClans);
                model.Tasks = model.Tasks.OrderBy(c => c.ClanName).ToList();
            }
            else
            {
                model.Tasks = new List<Models.UpdateTask>();
            }

            return model;
        }

        protected async void DeleteTasks()
        {
            foreach(var task in db.UpdateTasks)
            {
                db.UpdateTasks.Remove(task);
            }

            await db.SaveChangesAsync();
        }

        protected async Task<UpdateTaskResponse> UpdatePlayer(string playerTag)
        {
            var status = new UpdateTaskResponse();
            status.ID = playerTag;
            var playerName = playerTag;

            try
            {
                var newPlayer = await api.GetPlayer(playerTag);
                playerName = $"{newPlayer.Name} / {newPlayer.ClanName}";
                var oldPlayer = db.Players.SingleOrDefault(e => e.Tag == playerTag);
                if (oldPlayer == null)
                {
                    newPlayer.LastUpdated = DateTime.UtcNow;
                    db.Entry(newPlayer).State = EntityState.Added;
                }
                else
                {
                    if (oldPlayer.TownHallLevel != newPlayer.TownHallLevel)
                        db.Add(new PlayerEvent {
                            ClanTag = newPlayer.ClanTag,
                            PlayerTag = newPlayer.Tag,
                            EventDate = DateTime.UtcNow,
                            EventType = PlayerEventType.Townhall,
                            Value = newPlayer.TownHallLevel
                        });

                    /*
                    if (oldPlayer.WarStars != newPlayer.WarStars)
                        db.Add(new PlayerEvent {
                            ClanTag = newPlayer.ClanTag,
                            PlayerTag = newPlayer.Tag,
                            EventDate = DateTime.UtcNow,
                            EventType = PlayerEventType.Stars,
                            Value = newPlayer.WarStars - oldPlayer.WarStars
                        });
                    */

                    oldPlayer.AttackWins = newPlayer.AttackWins;
                    oldPlayer.BestTrophies = newPlayer.BestTrophies;
                    oldPlayer.DefenseWins = newPlayer.DefenseWins;
                    oldPlayer.TownHallLevel = newPlayer.TownHallLevel;
                    oldPlayer.WarStars = newPlayer.WarStars;
                    oldPlayer.LastUpdated = DateTime.UtcNow;

                    db.Entry(oldPlayer).State = EntityState.Modified;
                }

                db.SaveChanges();

                status.Message = playerName;
                status.Status = true;
            }
            catch(Exception e)
            {
                status.Message = string.Format("{0} Failed: {1}", playerName, e.Message);
                status.Status = false;
            }

            return status;
        }

        protected async Task<UpdateTaskResponse> PerformTask(string id)
        {
            var status = new UpdateTaskResponse();
            status.ID = id;

            string clanName = "";
            string clanTag = "";
            UpdateTaskMode updateMode = UpdateTaskMode.Update;

            try
            {
                var task = db.UpdateTasks.Single(t => t.ID == Guid.Parse(id));

                clanName = task.ClanName;
                clanTag = task.ClanTag;
                updateMode = task.Mode;

                db.UpdateTasks.Remove(task);
                db.SaveChanges();

                if (task.Mode == UpdateTaskMode.Delete)
                {
                    lock (lockObject)
                    {
                        var clan = db.Clans.Single(c => c.Tag == task.ClanTag);
                        if (clan != null)
                        {
                            var members = db.Members.Where(m => m.ClanTag == clan.Tag);

                            foreach (var member in members.ToList())
                                db.Entry(member).State = EntityState.Deleted;

                            db.Entry(clan).State = EntityState.Deleted;
                            db.SaveChanges();
                        }
                    }
                }

                if (task.Mode == UpdateTaskMode.Update)
                {
                    var clanEvent = new ClanEvent();
                    clanEvent.EventDate = DateTime.UtcNow;
                    clanEvent.ClanTag = task.ClanTag;
                    var utc = DateTime.UtcNow;
                    clanEvent.EventDate = new DateTime(utc.Year, utc.Month, utc.Day, utc.Hour, 0, 0, DateTimeKind.Utc);
                    var clan = await api.GetClan(task.ClanTag, true, true);
                    lock (lockObject)
                    {
                        clan.Group = task.ClanGroup;

                        var existingClanQ = from c in db.Clans
                                            where c.Tag == task.ClanTag
                                            select new
                                            {
                                                BadgeUrl = c.BadgeUrl,
                                                ClanLevel = c.ClanLevel,
                                                ClanPoints = c.ClanPoints,
                                                ClanType = c.Type,
                                                Description = c.Description,
                                                Group = c.Group,
                                                IsWarLogPublic = c.IsWarLogPublic,
                                                MemberCount = c.Members,
                                                Name = c.Name,
                                                RequiredTrophies = c.RequiredTrophies,
                                                WarLosses = c.WarLosses,
                                                WarTies = c.WarTies,
                                                WarWins = c.WarWins,
                                                WarWinStreak = c.WarWinStreak,
                                                Th11Count = c.Th11Count,
                                                Th10Count = c.Th10Count,
                                                Th9Count = c.Th9Count,
                                                Th8Count = c.Th8Count,
                                                ThLowCount = c.ThLowCount,
                                                WarCount = c.WarCount,
                                                MatchPercentage = c.MatchPercentage,
                                                WinPercentage = c.WinPercentage,
                                                EstimatedWeight = c.EstimatedWeight
                                            };

                        var existingClan = existingClanQ.ToList().First();

                        var clanModified = false;

                        if (clan.BadgeUrl != existingClan.BadgeUrl)
                            clanModified = true;
                        if (clan.ClanLevel != existingClan.ClanLevel)
                            clanModified = true;
                        if (clan.ClanPoints != existingClan.ClanPoints)
                            clanModified = true;
                        if (clan.Type != existingClan.ClanType)
                            clanModified = true;
                        if (clan.Description != existingClan.Description)
                            clanModified = true;
                        if (clan.Group != existingClan.Group)
                            clanModified = true;
                        if (clan.IsWarLogPublic != existingClan.IsWarLogPublic)
                            clanModified = true;
                        if (clan.Members != existingClan.MemberCount)
                            clanModified = true;
                        if (clan.Name != existingClan.Name)
                            clanModified = true;
                        if (clan.RequiredTrophies != existingClan.RequiredTrophies)
                            clanModified = true;
                        if (clan.WarLosses != existingClan.WarLosses)
                            clanModified = true;
                        if (clan.WarTies != existingClan.WarTies)
                            clanModified = true;
                        if (clan.WarWins != existingClan.WarWins)
                            clanModified = true;
                        if (clan.WarWinStreak != existingClan.WarWinStreak)
                            clanModified = true;

                        if (clanModified)
                        {
                            clan.Th11Count = existingClan.Th11Count;
                            clan.Th10Count = existingClan.Th10Count;
                            clan.Th9Count = existingClan.Th9Count;
                            clan.Th8Count = existingClan.Th8Count;
                            clan.ThLowCount = existingClan.ThLowCount;
                            clan.WarCount = existingClan.WarCount;
                            clan.MatchPercentage = existingClan.MatchPercentage;
                            clan.WinPercentage = existingClan.WinPercentage;
                            clan.EstimatedWeight = existingClan.EstimatedWeight;

                            db.Entry(clan).State = EntityState.Modified;
                        }

                        var oldMembers = (from m in db.Members
                                          where m.ClanTag == clan.Tag
                                          select new
                                          {
                                              Tag = m.Tag,
                                              BadgeUrl = m.BadgeUrl,
                                              ClanRank = m.ClanRank,
                                              ClanTag = m.ClanTag,
                                              Donations = m.Donations,
                                              DonationsReceived = m.DonationsReceived,
                                              ExpLevel = m.ExpLevel,
                                              Name = m.Name,
                                              Role = m.Role,
                                              Trophies = m.Trophies
                                          }).ToList();

                        if (clan.MemberList != null)
                        {
                            foreach (var clanMember in clan.MemberList.ToList())
                            {
                                var memberActive = false;
                                var oldMember1 = from c in oldMembers where c.Tag == clanMember.Tag select c;
                                if (oldMember1.Count() > 0)
                                {
                                    var oldMember = oldMember1.First();
                                    var modified = false;
                                    if (clanMember.BadgeUrl != oldMember.BadgeUrl)
                                        modified = true;
                                    if (clanMember.ClanRank != oldMember.ClanRank)
                                        modified = true;
                                    if (clanMember.ClanTag != oldMember.ClanTag)
                                        modified = true;
                                    if (clanMember.Donations != oldMember.Donations)
                                    {
                                        memberActive = true;
                                        if (clanMember.Donations > oldMember.Donations)
                                            clanEvent.Donations += (clanMember.Donations - oldMember.Donations);
                                        modified = true;
                                    }
                                    if (clanMember.DonationsReceived != oldMember.DonationsReceived)
                                    {
                                        modified = true;
                                        memberActive = true;
                                    }
                                    if (clanMember.ExpLevel != oldMember.ExpLevel)
                                        modified = true;
                                    if (clanMember.Name != oldMember.Name)
                                        modified = true;
                                    if (clanMember.Role != oldMember.Role)
                                    {
                                        var eventType = PlayerEventType.Promote;
                                        if (PlayerEvent.RoleToValue(oldMember.Role) > PlayerEvent.RoleToValue(clanMember.Role))
                                            eventType = PlayerEventType.Demote;

                                        db.Add(new PlayerEvent
                                        {
                                            ClanTag = clanTag,
                                            PlayerTag = clanMember.Tag,
                                            EventDate = DateTime.UtcNow,
                                            EventType = eventType,
                                            Role = clanMember.Role
                                        });
                                        modified = true;
                                    }
                                    if (clanMember.Trophies != oldMember.Trophies)
                                    {
                                        memberActive = true;
                                        modified = true;
                                    }
                                    if (modified)
                                    {
                                        db.Entry(clanMember).State = EntityState.Modified;
                                    }
                                }
                                else
                                {
                                    var oldMember = db.Members.Select(m => new { Tag = m.Tag, ClanTag = m.ClanTag }).FirstOrDefault(m => m.Tag == clanMember.Tag);
                                    if (oldMember != null)
                                    {
                                        db.Entry(clanMember).State = EntityState.Modified;

                                        db.Add(new PlayerEvent
                                        {
                                            ClanTag = oldMember.ClanTag,
                                            PlayerTag = clanMember.Tag,
                                            EventDate = DateTime.UtcNow,
                                            EventType = PlayerEventType.Leave
                                        });
                                    }
                                    else
                                    {
                                        memberActive = true;
                                        clanEvent.Donations += clanMember.Donations;
                                        db.Entry(clanMember).State = EntityState.Added;
                                    }

                                    db.Add(new PlayerEvent
                                    {
                                        ClanTag = clanTag,
                                        PlayerTag = clanMember.Tag,
                                        EventDate = DateTime.UtcNow,
                                        EventType = PlayerEventType.Join
                                    });

                                    if(clanMember.Role != "member")
                                    {
                                        db.Add(new PlayerEvent
                                        {
                                            ClanTag = clanTag,
                                            PlayerTag = clanMember.Tag,
                                            EventDate = DateTime.UtcNow,
                                            EventType = PlayerEventType.Promote,
                                            Role = clanMember.Role
                                        });
                                    }
                                }

                                if (memberActive)
                                    clanEvent.Activity++;
                            }

                            foreach (var clanMember in oldMembers)
                            {
                                var existingMember = (from m in clan.MemberList where m.Tag == clanMember.Tag select m).ToList();
                                if (existingMember.Count() == 0)
                                {
                                    var formerMember = new Member { Tag = clanMember.Tag };
                                    db.Entry(formerMember).State = EntityState.Deleted;

                                    db.Add(new PlayerEvent
                                    {
                                        ClanTag = clanTag,
                                        PlayerTag = clanMember.Tag,
                                        EventDate = DateTime.UtcNow,
                                        EventType = PlayerEventType.Leave
                                    });

                                }
                            }

                            db.Add(clanEvent);
                        }

                        if (clan.Wars != null)
                        {
                            var clanWars = (from w in db.Wars where w.ClanTag == clan.Tag select new { w.ID, w.Result, w.EndTime, w.OpponentTag, w.TeamSize, w.Friendly}).ToDictionary(w => w.ID);

                            foreach (var war in clan.Wars.ToList())
                            {
                                var earliestEndTime = war.EndTime.AddHours(-8); //Prepare for maintenance break
                                var latestEndTime = war.EndTime;
                                var duplicate = (from w in clanWars.Values
                                                 where w.ID != war.ID && 
                                                    w.EndTime > earliestEndTime && 
                                                    w.EndTime < latestEndTime &&
                                                    w.OpponentTag == war.OpponentTag &&
                                                    w.TeamSize == war.TeamSize &&
                                                    w.Friendly == false &&
                                                    war.Friendly == false
                                                 select w).FirstOrDefault();

                                if (clanWars.TryGetValue(war.ID, out var existingWar))
                                {
                                    if (!war.Result.Equals(existingWar.Result) || war.Result.Equals("inWar"))
                                        db.Entry(war).State = EntityState.Modified;
                                }
                                else
                                {
                                    db.Entry(war).State = EntityState.Added;
                                }

                                var addedMembers = new HashSet<string>();

                                if(war.Members != null && war.Members.Count > 0)
                                {
                                    var warMembers = (from m in db.WarMembers where m.WarID == war.ID select new { m.Tag, m.OpponentAttacks, m.ID }).ToDictionary(m => m.Tag, m => new { m.ID, m.OpponentAttacks });
                                    foreach(var member in war.Members)
                                    {
                                        addedMembers.Add(member.Tag);
                                        if(warMembers.TryGetValue(member.Tag, out var memberDetails))
                                        {
                                            if (memberDetails.OpponentAttacks != member.OpponentAttacks)
                                            {
                                                member.ID = memberDetails.ID;
                                                db.Entry(member).State = EntityState.Modified;
                                            }
                                        }
                                        else
                                        {
                                            db.Entry(member).State = EntityState.Added;
                                        }
                                    }
                                }

                                var addedAttacks = new HashSet<int>();

                                if (war.Attacks != null && war.Attacks.Count > 0)
                                {
                                    var warAttacks = (from a in db.WarAttacks where a.WarID == war.ID select a.Order).ToDictionary(m => m);
                                    foreach (var attack in war.Attacks)
                                    {
                                        addedAttacks.Add(attack.Order);
                                        if (!warAttacks.ContainsKey(attack.Order))
                                            db.Entry(attack).State = EntityState.Added;
                                    }
                                }

                                if (duplicate != null)
                                {
                                    var existingMembers = db.WarMembers.Where(w => w.WarID == war.ID);
                                    foreach(var existingMember in existingMembers.ToList())
                                    {
                                        if (!addedMembers.Contains(existingMember.Tag))
                                            addedMembers.Add(existingMember.Tag);
                                    }
                                    var duplicateMembers = db.WarMembers.Where(w => w.WarID == duplicate.ID);
                                    foreach(var member in duplicateMembers.ToList())
                                    {
                                        if (addedMembers.Contains(member.Tag))
                                        {
                                            db.Entry(member).State = EntityState.Deleted;
                                        }
                                        else
                                        {
                                            member.WarID = war.ID;
                                        }

                                    }

                                    var existingAttacks = db.WarAttacks.Where(w => w.WarID == war.ID);
                                    foreach (var existingAttack in existingAttacks.ToList())
                                    {
                                        if (!addedAttacks.Contains(existingAttack.Order))
                                            addedAttacks.Add(existingAttack.Order);
                                    }
                                    var duplicateAttacks = db.WarAttacks.Where(w => w.WarID == duplicate.ID);
                                    foreach (var attack in duplicateAttacks.ToList())
                                    {
                                        if (addedAttacks.Contains(attack.Order))
                                        {
                                            db.Entry(attack).State = EntityState.Deleted;
                                        }
                                        else
                                        {
                                            attack.WarID = war.ID;
                                        }

                                    }

                                    var duplicateWar = new War { ID = duplicate.ID };
                                    db.Entry(duplicateWar).State = EntityState.Deleted;
                                }
                            }
                        }

                        db.SaveChanges();
                    }
                }

                if (task.Mode == UpdateTaskMode.Insert)
                {
                    var clan = await api.GetClan(task.ClanTag, true, true);
                    if (clan == null)
                        throw new Exception(string.Format("Clan {0} not found", task.ClanTag));

                    lock (lockObject)
                    {

                        clan.Group = task.ClanGroup;

                        var existingMembers = (from m in db.Members where m.ClanTag != task.ClanTag select m.Tag).ToDictionary(m => m);

                        foreach (var clanMember in clan.MemberList)
                        {
                            //Member hopped from other clan
                            if (existingMembers.ContainsKey(clanMember.Tag))
                                db.Entry(clanMember).State = EntityState.Modified;
                        }

                        if (clan.Wars != null)
                        {
                            var existingWars = (from w in db.Wars where w.ClanTag == task.ClanTag select w.ID).ToDictionary(w => w);

                            foreach (var clanWar in clan.Wars)
                            {
                                if (!existingWars.ContainsKey(clanWar.ID))
                                    db.Wars.Add(clanWar);
                            }
                        }

                        db.Clans.Add(clan);

                        db.SaveChanges();
                    }
                }

                status.Message = clanName;
                status.Status = true;
            }
            catch (Exception e)
            {
                logger.LogError("PerformTask.Error: {0}", e.ToString());
                status.Message = string.Format("{0} {1} {2} Failed: {3}", clanTag, clanName, updateMode, e.Message);
                status.Status = false;
            }

            return status;
        }

        // GET: Update
        public async Task<IActionResult> Index()
        {
            logger.LogInformation("Index");

            IndexViewModel model = null;
            try
            {
                db.Database.Migrate();
                model = await GetUpdates();
            }
            catch (Exception e)
            {
                model = new IndexViewModel();
                model.Errors = new List<string>();
                model.Errors.Add(e.GetType().FullName);
                model.Errors.Add(e.Message);
                model.Errors.Add(e.StackTrace);
                model.Tasks = new List<UpdateTask>();
            }

            return View(model);
        }

        public async Task<IActionResult> GetTasks()
        {
            logger.LogInformation("GetTasks");
            return Json(await GetUpdates());
        }

        public async Task<IActionResult> UpdateTask(string id)
        {
            logger.LogInformation("UpdateTask {0}", id);
            try
            {
                return Json(await PerformTask(id));
            }
            catch(Exception e)
            {
                logger.LogError("UpdateTask.Error {0}: {1}", id, e.ToString());
                return Json(new UpdateTaskResponse { ID = id, Message = e.Message, Status = false });
            }
        }

        public async Task<IActionResult> UpdatePlayerTask(string id)
        {
            logger.LogInformation("UpdatePlayerTask {0}", id);

            var playerTag = Utils.LinkIdToTag(id);

            try
            {
                return Json(await UpdatePlayer(playerTag));
            }
            catch (Exception e)
            {
                logger.LogError("UpdatePlayerTask.Error {0}: {1}", id, e.Message);
                return Json(new UpdateTaskResponse { ID = id, Message = e.Message, Status = false });
            }
        }

        protected void PerformFinished()
        {
            lock (lockObject)
            {
                logger.LogInformation("PerformFinished.DeleteTasks");
                this.DeleteTasks();

                logger.LogInformation("PerformFinished.DeleteHistory");
                statistics.DeleteHistory();

                logger.LogInformation("PerformFinished.UpdateValidities");
                statistics.UpdateValidities();

                logger.LogInformation("PerformFinished.CalculateSyncs");
                statistics.CalculateSyncs();

                logger.LogInformation("PerformFinished.UpdateSyncMatch");
                statistics.UpdateSyncMatch();

                logger.LogInformation("PerformFinished.UpdateClanStats");
                statistics.UpdateClanStats();

                logger.LogInformation("PerformFinished.Done");
            }
        }

        public IActionResult Friendly()
        {
            logger.LogInformation("Friendly");
            var model = new IndexViewModel();
            model.Errors = new List<string>();
            var updates = 0;



            var prevEndTimes = new Dictionary<string, DateTime>();

            foreach (var war in db.Wars.OrderBy(w => w.EndTime).ToList())
            {
                if (prevEndTimes.TryGetValue(war.ClanTag, out DateTime prevEndTime))
                {
                    var hoursSinceLastWar = war.EndTime.Subtract(prevEndTime).TotalHours;
                    if (hoursSinceLastWar < 47.0 || (war.ClanExpEarned == 0 && war.ClanStars > 0))
                    {
                        if (!war.Friendly)
                        {
                            war.Friendly = true;
                            model.Errors.Add(string.Format("{0} vs {1} on {2} marked as friendly war", war.ClanName, war.OpponentName, war.EndTime));
                            updates++;
                            if(updates > 10)
                            {
                                db.SaveChanges();
                                updates = 0;
                            }
                        }
                    }
                }
                prevEndTimes[war.ClanTag] = war.EndTime;
            }

            db.SaveChanges();

            model.Tasks = new List<UpdateTask>();
            return View("Index", model);
        }

        public IActionResult Finish()
        {
            logger.LogInformation("Finish");
            var model = new IndexViewModel();
            model.Errors = new List<string>();
            model.Tasks = new List<UpdateTask>();
            return View("Index", model);
        }

        public IActionResult UpdateFinished()
        {
            logger.LogInformation("UpdateFinished.Begin");

            var status = new UpdateTaskResponse { Message = "Done", Status = true };

            try
            {
                this.PerformFinished();
            }
            catch(Exception e)
            {
                logger.LogError("UpdateFinished.Error: {0}", e.Message);

                status.Message = e.Message;
                status.Status = false;
            }

            logger.LogInformation("UpdateFinished.End");

            return Json(status);
        }

        public IActionResult PlayerBatch()
        {
            logger.LogInformation("PlayerBatch.Begin");

            /*
            try
            {
                var starEventCleanupDate = DateTime.UtcNow.AddDays(-14);
                var starEvents = db.PlayerEvents.Where(e => e.EventType == PlayerEventType.Stars && e.EventDate < starEventCleanupDate).Take(MAX_UPDATES);
                db.PlayerEvents.RemoveRange(starEvents);
                db.SaveChanges();
                logger.LogInformation("PlayerBatch.StarEventsDeleted");
            }
            catch( Exception e)
            {
                logger.LogWarning("PlayerBatch.StarEventDeleteFailed: {0}", e.Message);
            }*/

            var memberTags = db.Members.Where(m => !db.Players.Where(p => p.Tag == m.Tag).Any()).Select(m => m.Tag).Take(Constants.PLAYER_BATCH).ToList();

            logger.LogInformation("PlayerBatch.NewMembers = {0}", memberTags.Count);

            if (memberTags.Count < Constants.PLAYER_BATCH)
            {
                var playerTags = db.Players.Where(p => db.Members.Where(m => m.Tag == p.Tag).Any()).OrderBy(p => p.LastUpdated).Select(p => p.Tag).Take(Constants.PLAYER_BATCH - memberTags.Count).ToList();
                foreach (var playerTag in playerTags)
                    memberTags.Add(playerTag);
            }

            logger.LogInformation("PlayerBatch.End");

            return Json(memberTags);
        }

        public IActionResult Players()
        {
            logger.LogInformation("Players");

            var model = new PlayersViewModel();
            model.Errors = new List<string>();
            model.Tasks = new List<PlayerUpdateTask>();

            /*
            var playerEvent = db.PlayerEvents.FirstOrDefault();
            if(playerEvent == null)
            {
                foreach( var member in db.Members.Where(m => m.Role != "member").Select(m => new { Tag = m.Tag, ClanTag = m.ClanTag, Role = m.Role }).ToList())
                {
                    db.Add(new PlayerEvent
                    {
                        ClanTag = member.ClanTag,
                        PlayerTag = member.Tag,
                        EventDate = DateTime.UtcNow,
                        EventType = PlayerEventType.Promote,
                        Role = member.Role
                    });
                }
                db.SaveChanges();
            }*/

            foreach(var member in db.Members.Select(m => new { Tag = m.Tag, Name = m.Name }))
            {
                var task = new PlayerUpdateTask
                {
                    Tag = member.Tag,
                    Name = member.Name
                };
                model.Tasks.Add(task);
            }

            return View(model);
        }
    }
}