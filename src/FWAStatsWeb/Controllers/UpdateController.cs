using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FWAStatsWeb.Models;
using FWAStatsWeb.Models.UpdateViewModels;
using FWAStatsWeb.Data;
using FWAStatsWeb.Logic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Firebase.Database.Query;
using Firebase.Database;

namespace FWAStatsWeb.Controllers
{
    [ResponseCache(NoStore = true)]
    public class UpdateController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClanLoader loader;
        private readonly IClanUpdater updater;
        private readonly IClashApi api;
        private readonly IClanStatistics statistics;
        private readonly IMemberUpdater memberUpdater;
        ILogger<UpdateController> logger;
        IGoogleSheetsService googleSheets;
        IOptions<WeightDatabaseOptions> weightDatabase;
        IOptions<WeightResultOptions> resultDatabase;
        private readonly IOptions<StatisicsOptions> options;

        private static readonly object lockObject = new object();

        class PlayerWeight
        {
            public string Tag { get; set; }
            public string Th { get; set; }
            public string Extra { get; set; }
            public decimal? Loot { get; set; }
            public DateTime? Time { get; set; }
            public decimal? Wt { get; set; }
        }

        public UpdateController(
            ApplicationDbContext context,
            IClanLoader loader,
            IClanUpdater updater,
            IClashApi api,
            IClanStatistics statistics,
            IMemberUpdater memberUpdater,
            ILogger<UpdateController> logger,
            IGoogleSheetsService googleSheets,
            IOptions<WeightDatabaseOptions> weightDatabase,
            IOptions<WeightResultOptions> resultDatabase,
            IOptions<StatisicsOptions> options)
        {
            this.db = context;
            this.loader = loader;
            this.updater = updater;
            this.api = api;
            this.statistics = statistics;
            this.memberUpdater = memberUpdater;
            this.logger = logger;
            this.googleSheets = googleSheets;
            this.weightDatabase = weightDatabase;
            this.resultDatabase = resultDatabase;
            this.options = options;
        }

        protected async Task<IndexViewModel> GetUpdates()
        {
            var model = new IndexViewModel();

            var loadedClans = await loader.Load(Constants.LIST_FWA);

            model.Errors = loader.Errors;

            //Update all clans when list is empty
            if (loadedClans.Count == 0)
            {
                loadedClans = db.Clans.Select(v => new ClanObject { Tag = v.Tag, Name = v.Name, Group = Constants.LIST_FWA }).ToList();
            }

            if (loadedClans.Count > 0)
            {
                model.Tasks = updater.GetUpdates(loadedClans);
                model.Tasks = model.Tasks.OrderBy(c => c.ClanName).ToList();
            }
            else
            {
                model.Tasks = new List<UpdateTask>();
            }

            return model;
        }

        protected async Task DeleteTasks()
        {
            await db.Database.ExecuteSqlRawAsync("DELETE FROM UpdateTasks");
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

        protected async Task UpdateWeightsOld()
        {
            var data = await googleSheets.Get(weightDatabase.Value.SheetId, "ROWS", weightDatabase.Value.Range);
            if (data != null)
            {
                var weights = db.Weights.ToDictionary(w => w.Tag);
                var updates = 0;
                var dateZero = new DateTime(1899, 12, 30, 0, 0, 0);

                foreach (var row in data)
                {
                    var tag = "";
                    var weight = 0;
                    DateTime timestamp = DateTime.MinValue;

                    if (row.Count > weightDatabase.Value.TagColumn && row[weightDatabase.Value.TagColumn] != null)
                        tag = row[weightDatabase.Value.TagColumn].ToString();
                    if (row.Count > weightDatabase.Value.WeightColumn && row[weightDatabase.Value.WeightColumn] != null)
                        int.TryParse(row[weightDatabase.Value.WeightColumn].ToString(), out weight);
                    if (row.Count > weightDatabase.Value.TimestampColumn && row[weightDatabase.Value.TimestampColumn] != null)
                    {
                        try
                        {
                            timestamp = dateZero.AddDays(Convert.ToDouble(row[4]));
                        }
                        catch (Exception)
                        {
                            logger.LogError("Unable to convert to double: '{0}'", row[4]);
                            timestamp = new DateTime(1900, 1, 1);
                        }

                        //round to second to eliminate unnecessary updates
                        timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, timestamp.Second);
                        timestamp = timestamp.Subtract(Utils.EasternTimeZone.GetUtcOffset(timestamp));
                    }

                    if (weight <= 200)
                        weight *= 1000;

                    tag = Utils.LinkIdToTag(tag);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        if (weights.TryGetValue(tag, out var w))
                        {
                            if (weight != w.WarWeight && timestamp > w.LastModified)
                            {
                                logger.LogInformation("UpdateWeight: {0} {1} -> {2} ({3} > {4})",tag, w.WarWeight, weight, timestamp, w.LastModified);
                                w.WarWeight = weight;
                                w.ExtWeight = weight;
                                w.LastModified = timestamp;
                                updates++;
                            }
                            else if(weight != w.ExtWeight)
                            {
                                w.ExtWeight = weight;
                                updates++;
                            }
                        }
                        else
                        {
                            logger.LogInformation("InsertWeight: {0} {1} ({2})", tag, weight, timestamp);
                            var newWeight = new Weight { Tag = tag, WarWeight = weight, ExtWeight = weight, LastModified = timestamp };
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

        protected async Task UpdateWeights(bool fullUpdate)
        {
            var firebase = new FirebaseClient(weightDatabase.Value.Url);
            FirebaseQuery query = null;

            if(fullUpdate)
            {
                logger.LogInformation("Full weight search");
                query = firebase.Child(weightDatabase.Value.ResourceName);
            }
            else
            {
                var dateFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'";
                var sinceDate = DateTime.UtcNow.AddHours(-1 * weightDatabase.Value.SinceHours).ToString(dateFormat);

                logger.LogInformation("Weight search from {0}", sinceDate);

                query = firebase
                    .Child(weightDatabase.Value.ResourceName)
                    .OrderBy("time")
                    .StartAt(sinceDate);
            }

            var data = await query.OnceAsync<PlayerWeight>();

            if (data != null)
            {
                var weights = new Dictionary<string, Weight>();
                if(fullUpdate || data.Count > 100) //Cache all weights when doing full update
                    weights = db.Weights.ToDictionary(w => w.Tag);
                var updates = 0;
                var dateZero = new DateTime(1899, 12, 30, 0, 0, 0);

                foreach (var row in data.Where(r => r.Object != null))
                {
                    var tag = "";
                    var weight = 0;
                    DateTime timestamp = DateTime.MinValue;

                    tag = row.Key;
                    if (row.Object.Wt.HasValue)
                        weight = Convert.ToInt32(row.Object.Wt);
                    if (row.Object.Time.HasValue)
                        timestamp = row.Object.Time.Value.ToUniversalTime();

                    if (weight <= 200)
                        weight *= 1000;

                    tag = Utils.LinkIdToTag(tag);

                    if (!string.IsNullOrEmpty(tag))
                    {
                        if(!fullUpdate)
                        {
                            if(!weights.ContainsKey(tag))
                            {
                                var w1 = db.Weights.SingleOrDefault(t => t.Tag == tag);
                                if (w1 != null)
                                    weights.Add(tag, w1);
                            }
                        }
                        if (weights.TryGetValue(tag, out var w))
                        {
                            if (weight != w.WarWeight && timestamp > w.LastModified)
                            {
                                logger.LogInformation("UpdateWeight: {0} {1} -> {2} ({3} > {4})", tag, w.WarWeight, weight, timestamp, w.LastModified);
                                w.WarWeight = weight;
                                w.ExtWeight = weight;
                                w.LastModified = timestamp;
                                updates++;
                            }
                            else if (weight != w.ExtWeight)
                            {
                                w.ExtWeight = weight;
                                updates++;
                            }
                        }
                        else
                        {
                            logger.LogInformation("InsertWeight: {0} {1} ({2})", tag, weight, timestamp);
                            var newWeight = new Weight { Tag = tag, WarWeight = weight, ExtWeight = weight, LastModified = timestamp };
                            db.Weights.Add(newWeight);
                            weights.Add(tag, newWeight);
                            updates++;
                        }
                    }

                    if (updates > 100)
                    {
                        db.SaveChanges();
                        updates = 0;
                    }
                }
                db.SaveChanges();

                logger.LogInformation("{0} weights processed", data.Count);
            }
        }

        protected async Task UpdateResults()
        {
            logger.LogInformation("UpdateResults.ReadDB");

            var results = db.WeightResults.ToDictionary(r => r.Tag);

            var resultSet = new HashSet<string>();

            var dateZero = new DateTime(1899, 12, 30, 0, 0, 0);

            var pendingSet = new HashSet<string>();

            foreach (var resultDb in resultDatabase.Value)
            {
                logger.LogInformation("UpdateResults.ResultFetchBegin{0}", resultDb.TeamSize);

                var ranges = new string[] { resultDb.ResultRange, resultDb.PendingRange };

                var resultDataDict = await googleSheets.BatchGet(resultDb.SheetId, "ROWS", ranges);

                if (resultDataDict == null)
                    throw new Exception("BatchGet returned null");
                if (resultDataDict.Count != ranges.Length)
                    throw new Exception(string.Format("BatchGet returned {0} elements when {1} was expected", resultDataDict.Count, ranges.Length));

                var resultData = resultDataDict[0];
                logger.LogInformation("UpdateResults.ResultFetchEnd{0}", resultDb.TeamSize);
                if (resultData != null)
                {
                    foreach (var row in resultData)
                    {
                        if (row.Count > (resultDb.TeamSize + 12))
                        {
                            var clanTag = Utils.LinkIdToTag(Convert.ToString(row[4]));
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

                                try
                                {
                                    int dataOffset = 0;
                                    if (row.Count > (resultDb.TeamSize + 15))
                                        dataOffset = 3;

                                    DateTime timestamp;
                                    try
                                    {
                                        timestamp = dateZero.AddDays(Convert.ToDouble(row[0]));
                                    }
                                    catch (Exception)
                                    {
                                        logger.LogError("Unable to convert to double: '{0}'", row[0]);
                                        timestamp = new DateTime(1900, 1, 1);
                                    }

                                    //round to second to eliminate unnecessary updates
                                    timestamp = new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, timestamp.Second);
                                    timestamp = timestamp.Subtract(Utils.EasternTimeZone.GetUtcOffset(timestamp));

                                    if (timestamp > result.Timestamp)
                                    {
                                        result.Timestamp = timestamp;
                                        result.TeamSize = resultDb.TeamSize;

                                        //TODO
                                        result.TH14Count = 0;
                                        result.TH13Count = 0;
                                        result.TH12Count = 0;
                                        result.TH11Count = 0;
                                        result.TH10Count = 0;
                                        result.TH9Count = 0;
                                        result.TH8Count = 0;
                                        result.TH7Count = 0;

                                        var totalWeight = 0;

                                        int max = resultDb.TeamSize + 10;
                                        for (int i = 11; i <= max; i++)
                                        {
                                            var weight = Convert.ToInt32(row[i + dataOffset]);
                                            if (weight > Constants.MAXWEIGHT_TH14)
                                                weight = Constants.MAXWEIGHT_TH14;
                                            if (weight < 0)
                                                weight = 0;
                                            totalWeight += weight;

                                            if (weight > Constants.MAXWEIGHT_TH13)
                                                result.TH14Count++;
                                            else if (weight > Constants.MAXWEIGHT_TH12)
                                                result.TH13Count++;
                                            else if (weight > Constants.MAXWEIGHT_TH11)
                                                result.TH12Count++;
                                            else if (weight > Constants.MAXWEIGHT_TH10)
                                                result.TH11Count++;
                                            else if (weight > Constants.MAXWEIGHT_TH9)
                                                result.TH10Count++;
                                            else if (weight > Constants.MAXWEIGHT_TH8)
                                                result.TH9Count++;
                                            else if (weight > Constants.MAXWEIGHT_TH7)
                                                result.TH8Count++;
                                            else
                                                result.TH7Count++;

                                            result.SetBase(i - 10, weight);
                                        }

                                        for (int i = resultDb.TeamSize + 1; i <= Constants.WAR_SIZE2; i++)
                                        {
                                            result.SetBase(i, 0);
                                        }

                                        result.THSum = result.TH13Count * 13 + result.TH12Count * 12 + result.TH11Count * 11 + result.TH10Count * 10 + result.TH9Count * 9 + result.TH8Count * 8 + result.TH7Count * 7;

                                        result.Weight = totalWeight;
                                    }

                                }
                                catch(Exception e)
                                {
                                    logger.LogError("ClanResult {0}: {1}", clanTag, e.ToString());
                                }
                            }
                        }
                    }
                }

                var pendingData = resultDataDict[1];
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

            logger.LogInformation("UpdateResults.DeleteLoop");

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

            logger.LogInformation("UpdateResults.Save");

            await db.SaveChangesAsync();

            logger.LogInformation("UpdateResults.Finished");
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
                playerName = newPlayer.Name;
                var memberStatus = memberUpdater.UpdatePlayer(newPlayer, false);
                status.Message = memberStatus.Message;
                status.Status = memberStatus.Status;
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

                var keepWarsSince = DateTime.UtcNow.AddDays(-1.0 * options.Value.Wars);

                foreach (var clanWar in clan.Wars.Where(w => w.EndTime > keepWarsSince))
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
            existingClan.InLeague = clan.InLeague;

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
                        if (existingMember.Name != member.Name)
                        {
                            db.PlayerEvents.Add(new PlayerEvent
                            {
                                ClanTag = clan.Tag,
                                PlayerTag = member.Tag,
                                EventDate = DateTime.UtcNow,
                                EventType = PlayerEventType.NameChange,
                                StringValue = existingMember.Name,
                                Role = member.Role
                            });

                            var existingPlayer = db.Players.SingleOrDefault(p => p.Tag == member.Tag);
                            if(existingPlayer != null)
                            {
                                existingPlayer.Name = member.Name;
                            }
                        }
                        existingMember.LeagueName = member.LeagueName;
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

                            if (otherClanMember.Name != member.Name)
                            {
                                db.PlayerEvents.Add(new PlayerEvent
                                {
                                    ClanTag = otherClanMember.ClanTag,
                                    PlayerTag = member.Tag,
                                    EventDate = DateTime.UtcNow,
                                    EventType = PlayerEventType.NameChange,
                                    StringValue = otherClanMember.Name,
                                    Role = member.Role
                                });

                                var existingPlayer = db.Players.SingleOrDefault(p => p.Tag == member.Tag);
                                if (existingPlayer != null)
                                {
                                    existingPlayer.Name = member.Name;
                                }

                                otherClanMember.Name = member.Name;
                            }
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

            db.SaveChanges(); //clan and members

            if (clan.Wars != null)
            {
                var clanWars = (from w in db.Wars where w.ClanTag == clan.Tag select w).ToDictionary(w => w.ID);

                //Clean invalid wars
                var deleteInvalidWars = new List<string>();
                foreach (var w in clanWars)
                {
                    if (w.Value.Result == null || w.Value.OpponentTag == null)
                        deleteInvalidWars.Add(w.Key);
                }

                foreach(var k in deleteInvalidWars)
                {
                    var war = db.Wars.SingleOrDefault(w => w.ID == k);
                    if (war != null)
                        db.Wars.Remove(war);
                    db.SaveChanges();
                    clanWars.Remove(k);
                }
                //clean end

                var keepWarsSince = DateTime.UtcNow.AddDays(-1.0 * options.Value.Wars);

                foreach (var war in clan.Wars.Where(w => w.EndTime > keepWarsSince))
                {
                    var earliestEndTime = war.EndTime.AddHours(-3); //Prepare for maintenance break
                    var latestEndTime = war.EndTime.AddMinutes(1); //Prepare for 1 sec off
                    var duplicate = (from w in clanWars.Values
                                     where w.ID != war.ID &&
                                        w.Result != "win" && w.Result != "tie" && w.Result != "lose" && //Duplicate must be the incomplete one
                                        (war.Result == "win" || war.Result == "tie" || war.Result == "lose") && //and current war must be ended
                                        w.EndTime > earliestEndTime && //maintenance break
                                        w.EndTime < latestEndTime && //1 sec off
                                        w.OpponentTag == war.OpponentTag &&
                                        w.TeamSize == war.TeamSize &&
                                        w.Friendly == false &&
                                        war.Friendly == false
                                     select w).FirstOrDefault();

                    //Maintenance break on prepday or battle day
                    if(duplicate == null && (war.Result == "preparation" || war.Result == "inWar"))
                    {
                        duplicate = (from w in clanWars.Values
                                         where w.ID != war.ID &&
                                            (w.Result == "preparation" || w.Result == "inWar") &&
                                            w.EndTime > earliestEndTime && //maintenance break
                                            w.EndTime < war.EndTime && //look only past
                                            w.OpponentTag == war.OpponentTag &&
                                            w.TeamSize == war.TeamSize &&
                                            w.Friendly == false &&
                                            war.Friendly == false
                                         select w).FirstOrDefault();
                    }

                    if (duplicate != null)
                    {
                        if (war.PreparationStartTime == DateTime.MinValue)
                        {
                            war.PreparationStartTime = duplicate.PreparationStartTime;
                        }
                        if (war.StartTime == DateTime.MinValue)
                        {
                            war.StartTime = duplicate.StartTime;
                        }
                    }

                    if (clanWars.TryGetValue(war.ID, out var existingWar))
                    {
                        existingWar.ClanAttacks = war.ClanAttacks;
                        existingWar.ClanBadgeUrl = war.ClanBadgeUrl;
                        existingWar.ClanDestructionPercentage = war.ClanDestructionPercentage;
                        existingWar.ClanExpEarned = war.ClanExpEarned;
                        existingWar.ClanLevel = war.ClanLevel;
                        existingWar.ClanName = war.ClanName;
                        existingWar.ClanStars = war.ClanStars;
                        //existingWar.EndTime = war.EndTime;
                        existingWar.Friendly = war.Friendly;
                        existingWar.OpponentBadgeUrl = war.OpponentBadgeUrl;
                        existingWar.OpponentDestructionPercentage = war.OpponentDestructionPercentage;
                        existingWar.OpponentLevel = war.OpponentLevel;
                        existingWar.OpponentName = war.OpponentName;
                        existingWar.OpponentStars = war.OpponentStars;
                        if (war.PreparationStartTime > existingWar.PreparationStartTime)
                            existingWar.PreparationStartTime = war.PreparationStartTime;
                        if (existingWar.PreparationStartTime == DateTime.MinValue)
                            existingWar.PreparationStartTime = existingWar.SearchTime;
                        existingWar.Result = war.Result;
                        if (war.StartTime > existingWar.StartTime)
                            existingWar.StartTime = war.StartTime;
                        existingWar.TeamSize = war.TeamSize;
                    }
                    else
                    {
                        var tmpAttacks = war.Attacks;
                        var tmpMembers = war.Members;

                        //Detach child objects
                        war.Attacks = null;
                        war.Members = null;

                        db.Wars.Add(war);
                        db.SaveChanges();
                        db.Entry(war).State = EntityState.Detached;

                        //Attach child objects
                        war.Attacks = tmpAttacks;
                        war.Members = tmpMembers;
                    }

                    if (duplicate != null)
                    {
                        db.Database.ExecuteSqlRaw("UPDATE WarMembers SET WarID = {0} WHERE WarID = {1}", war.ID, duplicate.ID);
                        db.Database.ExecuteSqlRaw("UPDATE WarAttacks SET WarID = {0} WHERE WarID = {1}", war.ID, duplicate.ID);
                        db.Wars.Remove(duplicate);
                    }

                    if (war.Members != null && war.Members.Count > 0)
                    {
                        //For some reason we got duplicates...
                        var warMembers = new Dictionary<string, WarMember>();
                        foreach (var m in db.WarMembers.Where(m => m.WarID == war.ID))
                        {
                            if (warMembers.ContainsKey(m.Tag))
                                db.WarMembers.Remove(m);
                            else
                                warMembers.Add(m.Tag, m);
                        }

                        foreach (var member in war.Members)
                        {
                            if (warMembers.TryGetValue(member.Tag, out var memberDetails))
                            {
                                memberDetails.OpponentAttacks = member.OpponentAttacks;
                                memberDetails.TownHallLevel = member.TownHallLevel;
                            }
                            else
                            {
                                db.WarMembers.Add(member);   
                            }
                        }

                        if (warMembers.Count > 0) //Note: This is not executed on first update
                        {
                            var clanPlayers = (from m in db.WarMembers from p in db.Players where m.WarID == war.ID && m.Tag == p.Tag && m.TownHallLevel > p.TownHallLevel select new { p, m }).ToList();
                            foreach (var player in clanPlayers)
                            {
                                if (!player.m.IsOpponent)
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
                    }

                    if (war.Attacks != null && war.Attacks.Count > 0)
                    {
                        //For some reason we got duplicates...
                        var warAttacks = new Dictionary<int, WarAttack>();
                        foreach (var a in db.WarAttacks.Where(m => m.WarID == war.ID))
                        {
                            if (warAttacks.ContainsKey(a.Order))
                                db.WarAttacks.Remove(a);
                            else
                                warAttacks.Add(a.Order, a);
                        }
                        foreach (var attack in war.Attacks)
                        {
                            if(!warAttacks.ContainsKey(attack.Order))
                                db.WarAttacks.Add(attack);
                        }
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
                var inner = e.InnerException;
                while(inner != null)
                {
                    logger.LogError("PerformTask.Inner: {0}", inner.Message);
                    inner = inner.InnerException;
                }
            }

            return status;
        }

        // GET: Update
        [ResponseCache(NoStore = true)]
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

        [ResponseCache(NoStore = true)]
        public IActionResult Clan(string id)
        {
            var tag = Utils.LinkIdToTag(id);

            var clan = db.ClanValidities.FirstOrDefault(c => c.Tag == tag);

            var model = new IndexViewModel
            {
                Errors = new List<string>(),
                Tasks = new List<UpdateTask>()
            };

            if (clan != null && clan.IsValid())
            {
                var task = new UpdateTask
                {
                    ID = Guid.NewGuid(),
                    ClanTag = clan.Tag,
                    ClanName = clan.Name,
                    ClanGroup = clan.Group,
                    Mode = UpdateTaskMode.Update
                };
                model.Tasks.Add(task);
                db.UpdateTasks.Add(task);
                db.SaveChanges();
            }
            else
            {
                if(clan == null)
                {
                    model.Errors.Add(string.Format("Clan '{0}' not found", id));
                }
                else
                {
                    model.Errors.Add(string.Format("Clan '{0}' is not valid", clan.Name));
                }
            }

            return View("Index", model);
        }

        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> Task(string id)
        {
            var model = new IndexViewModel
            {
                Errors = new List<string>(),
                Tasks = new List<UpdateTask>()
            };

            var task = id?.ToLowerInvariant() ?? "";

            if(task == "deletehistory")
            {
                try
                {
                    logger.LogInformation("Task.DeleteHistory");
                    statistics.DeleteHistory();
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "updatevalidities")
            {
                try
                {
                    logger.LogInformation("Task.UpdateValidities");
                    statistics.UpdateValidities();
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "calculatesyncs")
            {
                try
                {
                    logger.LogInformation("Task.CalculateSyncs");
                    await statistics.CalculateSyncs();
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "updatesyncmatch")
            {
                try
                {
                    logger.LogInformation("Task.UpdateSyncMatch");
                    statistics.UpdateSyncMatch();
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "updateclanstats")
            {
                try
                {
                    logger.LogInformation("Task.UpdateClanStats");
                    statistics.UpdateClanStats();
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "blacklisted")
            {
                try
                {
                    logger.LogInformation("Task.Blacklisted");
                    await this.UpdateBlacklisted();
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "weights")
            {
                try
                {
                    logger.LogInformation("Task.Weights");
                    await this.UpdateWeights(false);
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "allweights")
            {
                try
                {
                    logger.LogInformation("Task.AllWeights");
                    await this.UpdateWeights(true);
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            if (task == "updateresults")
            {
                try
                {
                    logger.LogInformation("Task.UpdateResults");
                    await this.UpdateResults();
                }
                catch (Exception e)
                {
                    model.Errors.Add(e.Message);
                    logger.LogError(e.ToString());
                }
            }

            return View("Index", model);
        }

        [ResponseCache(NoStore = true)]
        public async Task<IActionResult> GetTasks()
        {
            logger.LogInformation("GetTasks");
            return Json(await GetUpdates());
        }

        public async Task<IActionResult> UpdateTask(string id)
        {
            //logger.LogInformation("UpdateTask {0}", id);
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
            //logger.LogInformation("UpdatePlayerTask {0}", id);

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
            try
            {
                logger.LogInformation("PerformFinished.DeleteTasks");
                await this.DeleteTasks();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            try
            {
                logger.LogInformation("PerformFinished.DeleteHistory");
                statistics.DeleteHistory();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            try
            {
                logger.LogInformation("PerformFinished.UpdateValidities");
                statistics.UpdateValidities();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            try
            {
                logger.LogInformation("PerformFinished.CalculateSyncs");
                await statistics.CalculateSyncs();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }


            try
            {
                logger.LogInformation("PerformFinished.UpdateSyncMatch");
                statistics.UpdateSyncMatch();
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            try
            {
                logger.LogInformation("PerformFinished.UpdateClanStats");
                statistics.UpdateClanStats();
            }
            catch (Exception e)
            {

                logger.LogError(e.ToString());
            }

            try
            {
                logger.LogInformation("PerformFinished.Blacklisted");
                await this.UpdateBlacklisted();
            }
            catch(Exception e)
            {
                logger.LogError(e.ToString());
            }

            try
            {
                logger.LogInformation("PerformFinished.Weights");
                var fullUpdate = false;
                if (weightDatabase.Value.SinceHours == 0)
                    fullUpdate = true;
                var now = DateTime.UtcNow;
                if (now.DayOfWeek == DayOfWeek.Monday && now.Hour == 0 && now.Minute < 15)
                    fullUpdate = true;
                await this.UpdateWeights(fullUpdate);
            }
            catch (Exception e)
            {
                logger.LogError(e.ToString());
            }

            try
            {
                logger.LogInformation("PerformFinished.UpdateResults");
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

            foreach (var member in db.Members.Select(m => new { m.Tag, m.Name }))
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