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
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace LWFStatsWeb.Controllers
{
    public class UpdateController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClanLoader loader;
        private readonly IClanUpdater updater;
        private readonly IClashApi api;
        private readonly IClanStatistics statistics;
        ILogger<UpdateController> logger;
        private IMemoryCache memoryCache;
        IGoogleSheetsService googleSheets;
        IOptions<WeightDatabaseOptions> weightDatabase;
        IOptions<WeightResultOptions> resultDatabase;

        private static object lockObject = new object();

        public UpdateController(
            ApplicationDbContext context, 
            IClanLoader loader,
            IClanUpdater updater,
            IClashApi api,
            IClanStatistics statistics,
            IMemoryCache memoryCache,
            ILogger<UpdateController> logger,
            IGoogleSheetsService googleSheets,
            IOptions<WeightDatabaseOptions> weightDatabase,
            IOptions<WeightResultOptions> resultDatabase)
        {
            this.db = context;
            this.loader = loader;
            this.updater = updater;
            this.api = api;
            this.statistics = statistics;
            this.memoryCache = memoryCache;
            this.logger = logger;
            this.googleSheets = googleSheets;
            this.weightDatabase = weightDatabase;
            this.resultDatabase = resultDatabase;
        }

        protected async Task<IndexViewModel> GetUpdates()
        {
            var model = new IndexViewModel();

            var loadedClans = await loader.Load(Constants.LIST_FWA);

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

        protected async Task DeleteTasks()
        {
            foreach(var task in db.UpdateTasks)
            {
                db.UpdateTasks.Remove(task);
            }

            await db.SaveChangesAsync();
        }

        protected async Task UpdateBlacklisted()
        {
            var loadedClans = await loader.Load(Constants.LIST_BLACKLISTED);

            var newClans = loadedClans.ToDictionary(c => c.Tag, c => c.Name);

            var existingClans = db.BlacklistedClans.ToDictionary(c => c.Tag);

            foreach (var clan in existingClans.Values)
            {
                if(!newClans.ContainsKey(clan.Tag))
                {
                    db.BlacklistedClans.Remove(clan);
                }
            }

            foreach(var clan in newClans)
            {
                if(!existingClans.ContainsKey(clan.Key))
                {
                    var newClan = new BlacklistedClan { Tag = clan.Key, Name = clan.Value };
                    db.BlacklistedClans.Add(newClan);
                    existingClans.Add(clan.Key, newClan);
                }
            }

            db.SaveChanges();
        }

        protected async Task UpdateWeights()
        {
            var data = await googleSheets.Get(weightDatabase.Value.SheetId, "ROWS", weightDatabase.Value.Range);

            if(data != null)
            {
                var weights = db.Weights.ToDictionary(w => w.Tag);
                var updates = 0;

                foreach(var row in data)
                {
                    var tag = "";
                    var weight = 0;

                    if (row.Count > weightDatabase.Value.TagColumn && row[weightDatabase.Value.TagColumn] != null)
                        tag = row[weightDatabase.Value.TagColumn].ToString();
                    if (row.Count > weightDatabase.Value.WeightColumn && row[weightDatabase.Value.WeightColumn] != null)
                        int.TryParse(row[weightDatabase.Value.WeightColumn].ToString(), out weight);

                    if (weight <= 110)
                        weight *= 1000;

                    tag = Utils.LinkIdToTag(tag);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (weights.TryGetValue(tag, out var w))
                        {
                            if (weight > w.WarWeight && weight <= 110000)
                            {
                                w.WarWeight = weight;
                                w.LastModified = DateTime.UtcNow;
                                updates++;
                            }
                        }
                        else
                        {
                            var newWeight = new Weight { Tag = tag, WarWeight = weight, LastModified = DateTime.UtcNow };
                            db.Weights.Add(newWeight);
                            weights.Add(tag, newWeight);
                            updates++;
                        }
                    }

                    if(updates > 100)
                    {
                        db.SaveChanges();
                        updates = 0;
                    }
                }

                db.SaveChanges();
            }

        }

        protected async Task UpdateResults()
        {
            var results = db.WeightResults.ToDictionary(r => r.Tag);

            var resultSet = new HashSet<string>();

            var dateZero = new DateTime(1899, 12, 30, 0, 0, 0);
            var sheetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            var pendingSet = new HashSet<string>();

            foreach (var resultDb in resultDatabase.Value)
            {
                var resultData = await googleSheets.Get(resultDb.SheetId, "ROWS", resultDb.ResultRange);
                if (resultData != null)
                {
                    foreach (var row in resultData)
                    {
                        if (row.Count > (resultDb.TeamSize + 12))
                        {
                            var clanTag = Utils.LinkIdToTag((string)row[4]);
                            if (!string.IsNullOrEmpty(clanTag))
                            {
                                if (!resultSet.Contains(clanTag))
                                    resultSet.Add(clanTag);

                                if (!results.TryGetValue(clanTag, out WeightResult result))
                                {
                                    result = new WeightResult { Tag = clanTag };
                                    db.WeightResults.Add(result);
                                    results.Add(clanTag, result);
                                }

                                var timestamp = dateZero.AddDays((double)row[0]);

                                result.TeamSize = resultDb.TeamSize;

                                //round to second to eliminate unnecessary updates
                                result.Timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, timestamp.Second);

                                result.Timestamp = result.Timestamp.Subtract(sheetTimeZone.GetUtcOffset(result.Timestamp));

                                result.TH11Count = Convert.ToInt32(row[6]);
                                result.TH10Count = Convert.ToInt32(row[7]);
                                result.TH9Count = Convert.ToInt32(row[8]);
                                result.TH8Count = Convert.ToInt32(row[9]);
                                result.TH7Count = Convert.ToInt32(row[10]);

                                result.THSum = result.TH11Count * 11 + result.TH10Count * 10 + result.TH9Count * 9 + result.TH8Count * 8 + result.TH7Count * 7;

                                int max = resultDb.TeamSize + 10;
                                for (int i = 11; i <= max; i++)
                                {
                                    result.SetBase(i - 10, Convert.ToInt32(row[i]));
                                }

                                result.Weight = Convert.ToInt32(row[resultDb.TeamSize + 11]);
                            }
                        }
                    }
                }

                var pendingData = await googleSheets.Get(resultDb.SheetId, "ROWS", resultDb.PendingRange);
                if (pendingData != null)
                {
                    foreach (var row in pendingData)
                    {
                        if (row.Count > 0)
                        {
                            var clanTag = Utils.LinkIdToTag((string)row[0]);
                            if (!string.IsNullOrEmpty(clanTag) && !pendingSet.Contains(clanTag))
                            {
                                pendingSet.Add(clanTag);
                                if (!results.ContainsKey(clanTag))
                                {
                                    var result = new WeightResult { Tag = clanTag, Timestamp = DateTime.MinValue };
                                    db.WeightResults.Add(result);
                                    results.Add(clanTag, result);
                                    if (!resultSet.Contains(clanTag))
                                    {
                                        resultSet.Add(clanTag);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            foreach (var result in results)
            {
                if (!resultSet.Contains(result.Key))
                {
                    db.WeightResults.Remove(result.Value);
                }
                else
                {
                    result.Value.PendingResult = pendingSet.Contains(result.Key);
                }
            }

            await db.SaveChangesAsync();
        }

        protected async Task<UpdateTaskResponse> UpdatePlayer(string playerTag)
        {
            var status = new UpdateTaskResponse
            {
                ID = playerTag
            };
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

        protected void PerformDelete(string clanTag)
        {
            var clan = db.Clans.SingleOrDefault(c => c.Tag == clanTag);
            if (clan != null)
            {
                var members = db.Members.Where(m => m.ClanTag == clan.Tag).ToList();
                db.Members.RemoveRange(members);

                db.Clans.Remove(clan);

                db.SaveChanges();
            }
        }

        protected void PerformInsert(Clan clan)
        {
            var existingMembers = (from m in db.Members where m.ClanTag != clan.Tag select m.Tag).ToDictionary(m => m);

            foreach (var clanMember in clan.MemberList)
            {
                //Member hopped from other clan
                if (existingMembers.ContainsKey(clanMember.Tag))
                    db.Entry(clanMember).State = EntityState.Modified;
            }

            if (clan.Wars != null)
            {
                var existingWars = (from w in db.Wars where w.ClanTag == clan.Tag select w.ID).ToDictionary(w => w);

                foreach (var clanWar in clan.Wars)
                {
                    if (!existingWars.ContainsKey(clanWar.ID))
                        db.Wars.Add(clanWar);
                }
            }

            db.Clans.Add(clan);

            db.SaveChanges();
        }

        protected void PerformUpdate(Clan clan)
        {
            var existingClan = db.Clans.SingleOrDefault(c => c.Tag == clan.Tag);

            existingClan.Group = clan.Group;
            existingClan.BadgeUrl = clan.BadgeUrl;
            existingClan.ClanLevel = clan.ClanLevel;
            existingClan.ClanPoints = clan.ClanPoints;
            existingClan.Type = clan.Type;
            existingClan.Description = clan.Description;
            existingClan.IsWarLogPublic = clan.IsWarLogPublic;
            existingClan.Members = clan.Members;
            existingClan.Name = clan.Name;
            existingClan.RequiredTrophies = clan.RequiredTrophies;
            existingClan.WarLosses = clan.WarLosses;
            existingClan.WarTies = clan.WarTies;
            existingClan.WarWins = clan.WarWins;
            existingClan.WarWinStreak = clan.WarWinStreak;

            if (clan.MemberList != null)
            {
                var existingMembers = db.Members.Where(m => m.ClanTag == existingClan.Tag).ToDictionary(m => m.Tag);

                foreach (var member in clan.MemberList)
                {
                    //old clan member -> update fields
                    if (existingMembers.TryGetValue(member.Tag, out Member existingMember))
                    {
                        existingMember.BadgeUrl = member.BadgeUrl;
                        existingMember.ClanRank = member.ClanRank;
                        existingMember.Donations = member.Donations;
                        existingMember.DonationsReceived = member.DonationsReceived;
                        existingMember.ExpLevel = member.ExpLevel;
                        existingMember.Name = member.Name;
                        existingMember.Trophies = member.Trophies;
                        if (existingMember.Role != member.Role)
                        {
                            var eventType = PlayerEventType.Promote;
                            if (PlayerEvent.RoleToValue(existingMember.Role) > PlayerEvent.RoleToValue(member.Role))
                                eventType = PlayerEventType.Demote;

                            db.PlayerEvents.Add(new PlayerEvent
                            {
                                ClanTag = clan.Tag,
                                PlayerTag = member.Tag,
                                EventDate = DateTime.UtcNow,
                                EventType = eventType,
                                Role = member.Role
                            });
                        }
                        existingMember.Role = member.Role;
                    }
                    else //new clan member
                    {
                        //member hopped from other alliance clan
                        var otherClanMember = db.Members.Where(m => m.Tag == member.Tag).SingleOrDefault();
                        if (otherClanMember != null)
                        {
                            db.PlayerEvents.Add(new PlayerEvent
                            {
                                ClanTag = otherClanMember.ClanTag,
                                PlayerTag = otherClanMember.Tag,
                                EventDate = DateTime.UtcNow,
                                EventType = PlayerEventType.Leave
                            });

                            otherClanMember.ClanTag = member.ClanTag;
                        }
                        else
                        {
                            db.Members.Add(member);
                        }

                        db.PlayerEvents.Add(new PlayerEvent
                        {
                            ClanTag = clan.Tag,
                            PlayerTag = member.Tag,
                            EventDate = DateTime.UtcNow,
                            EventType = PlayerEventType.Join
                        });

                        //member is promoted after joining
                        if (member.Role != "member")
                        {
                            db.PlayerEvents.Add(new PlayerEvent
                            {
                                ClanTag = clan.Tag,
                                PlayerTag = member.Tag,
                                EventDate = DateTime.UtcNow,
                                EventType = PlayerEventType.Promote,
                                Role = member.Role
                            });
                        }
                    }
                }

                var currentMemberTags = clan.MemberList.Select(m => m.Tag).ToDictionary(m => m);

                //chech if members have left clan
                foreach (var existingMember in existingMembers.Values)
                {
                    if (!currentMemberTags.ContainsKey(existingMember.Tag))
                    {
                        db.PlayerEvents.Add(new PlayerEvent
                        {
                            ClanTag = clan.Tag,
                            PlayerTag = existingMember.Tag,
                            EventDate = DateTime.UtcNow,
                            EventType = PlayerEventType.Leave
                        });

                        db.Members.Remove(existingMember);
                    }
                }
            }

            //db.SaveChanges(); //clan and members

            if (clan.Wars != null)
            {
                var clanWars = (from w in db.Wars where w.ClanTag == clan.Tag select new { w.ID, w.Result, w.EndTime, w.OpponentTag, w.TeamSize, w.Friendly, w.Matched, w.Synced }).ToDictionary(w => w.ID);

                foreach (var war in clan.Wars)
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
                        {
                            war.Matched = existingWar.Matched;
                            war.Synced = existingWar.Synced;
                            db.Entry(war).State = EntityState.Modified;
                        }
                    }
                    else
                    {
                        db.Entry(war).State = EntityState.Added;
                    }

                    var addedMembers = new HashSet<string>();

                    if (war.Members != null && war.Members.Count > 0)
                    {
                        var warMembers = (from m in db.WarMembers where m.WarID == war.ID select new { m.Tag, m.OpponentAttacks, m.ID }).ToDictionary(m => m.Tag, m => new { m.ID, m.OpponentAttacks });
                        foreach (var member in war.Members)
                        {
                            addedMembers.Add(member.Tag);
                            if (warMembers.TryGetValue(member.Tag, out var memberDetails))
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

                        if (warMembers.Count > 0) //Note: This is not executed on first update
                        {
                            var clanPlayers = (from m in db.WarMembers from p in db.Players where m.WarID == war.ID && m.Tag == p.Tag && m.TownHallLevel > p.TownHallLevel select new { p, m }).ToList();
                            foreach (var player in clanPlayers)
                            {
                                player.p.TownHallLevel = player.m.TownHallLevel;

                                db.PlayerEvents.Add(new PlayerEvent
                                {
                                    ClanTag = clan.Tag,
                                    PlayerTag = player.p.Tag,
                                    EventDate = DateTime.UtcNow,
                                    EventType = PlayerEventType.Townhall,
                                    Value = player.p.TownHallLevel
                                });

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
                        var existingMembers = db.WarMembers.Where(w => w.WarID == war.ID).ToList();
                        foreach (var existingMember in existingMembers)
                        {
                            if (!addedMembers.Contains(existingMember.Tag))
                                addedMembers.Add(existingMember.Tag);
                        }
                        var duplicateMembers = db.WarMembers.Where(w => w.WarID == duplicate.ID).ToList();
                        foreach (var member in duplicateMembers)
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

                        var existingAttacks = db.WarAttacks.Where(w => w.WarID == war.ID).ToList();
                        foreach (var existingAttack in existingAttacks)
                        {
                            if (!addedAttacks.Contains(existingAttack.Order))
                                addedAttacks.Add(existingAttack.Order);
                        }
                        var duplicateAttacks = db.WarAttacks.Where(w => w.WarID == duplicate.ID).ToList();
                        foreach (var attack in duplicateAttacks)
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

            db.SaveChanges(); // war and attacks
        }

        protected async Task<UpdateTaskResponse> PerformTask(string id)
        {
            var status = new UpdateTaskResponse { ID = id };
            var task = new UpdateTask();

            try
            {
                task = db.UpdateTasks.SingleOrDefault(t => t.ID == Guid.Parse(id));

                db.UpdateTasks.Remove(task);

                if (task.Mode == UpdateTaskMode.Delete)
                {
                    lock (lockObject)
                        this.PerformDelete(task.ClanTag);
                }

                if (task.Mode == UpdateTaskMode.Update)
                {
                    var modifiedClan = await api.GetClan(task.ClanTag, true, true);
                    modifiedClan.Group = task.ClanGroup;
                    lock (lockObject)
                        this.PerformUpdate(modifiedClan);
                }

                if (task.Mode == UpdateTaskMode.Insert)
                {
                    var clan = await api.GetClan(task.ClanTag, true, true);
                    if (clan == null)
                        throw new Exception(string.Format("Clan {0} not found", task.ClanTag));

                    clan.Group = task.ClanGroup;
                    lock (lockObject)
                        this.PerformInsert(clan);
                    
                }

                status.Message = task.ClanName;
                status.Status = true;
            }
            catch (Exception e)
            {
                logger.LogError("PerformTask.Error: {0}", e.ToString());
                status.Message = string.Format("{0} {1} {2} Failed: {3}", task.ClanTag, task.ClanName, task.Mode, e.Message);
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
                model = new IndexViewModel()
                {
                    Errors = new List<string>() {
                        e.GetType().FullName,
                        e.Message,
                        e.StackTrace },
                    Tasks = new List<UpdateTask>()
                };
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

        protected async Task PerformFinished()
        {
            logger.LogInformation("PerformFinished.DeleteTasks");
            await this.DeleteTasks();

            logger.LogInformation("PerformFinished.DeleteHistory");
            statistics.DeleteHistory();

            logger.LogInformation("PerformFinished.UpdateValidities");
            statistics.UpdateValidities();

            logger.LogInformation("PerformFinished.CalculateSyncs");
            statistics.CalculateSyncs();

            logger.LogInformation("PerformFinished.UpdateSyncCalendar");
            try
            {
                await statistics.UpdateSyncCalendar();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            logger.LogInformation("PerformFinished.UpdateSyncMatch");
            statistics.UpdateSyncMatch();

            logger.LogInformation("PerformFinished.UpdateClanStats");
            statistics.UpdateClanStats();

            memoryCache.Remove(Constants.CACHE_HOME_INDEX);
            memoryCache.Remove(Constants.CACHE_CLANS_ALL);
            memoryCache.Remove(Constants.CACHE_CLANS_FOLLOWING);
            memoryCache.Remove(Constants.CACHE_CLANS_DEPARTED);
            memoryCache.Remove(Constants.CACHE_SYNCS_ALL);
            memoryCache.Remove(Constants.CACHE_DATA_CLANS);

            logger.LogInformation("PerformFinished.Blacklisted");
            try
            {
                await this.UpdateBlacklisted();
            }
            catch(Exception e)
            {
                logger.LogError(e.ToString());
            }

            logger.LogInformation("PerformFinished.Weights");
            try
            {
                await this.UpdateWeights();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            logger.LogInformation("PerformFinished.UpdateResults");
            try
            {
                await this.UpdateResults();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            logger.LogInformation("PerformFinished.Done");
        }

        public IActionResult Friendly()
        {
            logger.LogInformation("Friendly");
            var model = new IndexViewModel
            {
                Errors = new List<string>()
            };
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
            var model = new IndexViewModel
            {
                Errors = new List<string>(),
                Tasks = new List<UpdateTask>()
            };
            return View("Index", model);
        }

        public async Task<IActionResult> UpdateFinished()
        {
            logger.LogInformation("UpdateFinished.Begin");

            var status = new UpdateTaskResponse { Message = "Done", Status = true };

            try
            {
                await this.PerformFinished();
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

            var model = new PlayersViewModel
            {
                Errors = new List<string>(),
                Tasks = new List<PlayerUpdateTask>()
            };

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

            foreach (var member in db.Members.Select(m => new { Tag = m.Tag, Name = m.Name }))
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