using Ical.Net;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class StatisicsOptions
    {
        public int Wars { get; set; }
        public int Members { get; set; }
        public string SyncURL { get; set; }
    }

    public interface IClanStatistics
    {
        void DeleteHistory();
        void UpdateValidities();
        Task CalculateSyncs();
        void UpdateSyncMatch();
        void UpdateClanStats();
    }

    public class ClanStatistics : IClanStatistics
    {
        private readonly ApplicationDbContext db;
        private readonly IOptions<StatisicsOptions> options;
        ILogger<ClanStatistics> logger;

        public ClanStatistics(
            ApplicationDbContext db,
            IOptions<StatisicsOptions> options,
            ILogger<ClanStatistics> logger
            )
        {
            this.db = db;
            this.options = options;
            this.logger = logger;
        }

        public async Task CalculateSyncs()
        {
            var request = WebRequest.Create(options.Value.SyncURL);
            var response = await request.GetResponseAsync();
            var data = string.Empty;
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                data = await reader.ReadToEndAsync();
            }

            var syncDuration = new TimeSpan(2, 0, 0);

            this.FillSyncsUtc();

            var syncTimes = db.WarSyncs.Select(s => s.Start).ToHashSet();

            var cals = Calendar.Load(data);
            foreach (var cal in cals)
            {
                foreach (var syncEvent in cal.Events.Where(a => a.Duration == syncDuration).OrderBy(a => a.Start))
                {
                    var eventStart = syncEvent.Start.AsUtc;
                    var eventEnd = syncEvent.End.AsUtc;

                    if (!syncTimes.Contains(eventStart))
                    {
                        if (eventStart < DateTime.UtcNow)
                        {
                            var sync = new WarSync { Start = eventStart, Finish = eventEnd };
                            db.WarSyncs.Add(sync);
                        }
                    }
                }
            }

            db.SaveChanges();
        }

        public void UpdateValidities()
        {
            var currentClans = db.Clans.ToDictionary(c => c.Tag);
            var validClans = db.ClanValidities.ToDictionary(l => l.Tag);

            //Deleted clans
            foreach (var clan in validClans)
            {
                if(!currentClans.ContainsKey(clan.Key))
                {
                    var validClan = clan.Value;
                    if (validClan.ValidTo > DateTime.UtcNow)
                    {
                        validClan.ValidTo = DateTime.UtcNow;
                    }
                }
            }

            //New or existing clans
            foreach(var clan in currentClans)
            {
                var currentClan = clan.Value;
                if (validClans.TryGetValue(clan.Key, out ClanValidity validClan))
                {
                    if (validClan.ValidTo < DateTime.UtcNow)
                    {
                        validClan.ValidTo = DateTime.MaxValue;
                    }
                }
                else
                {
                    validClan = new ClanValidity() { Tag = clan.Key, Name = currentClan.Name, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.MaxValue, Group = currentClan.Group };
                    db.ClanValidities.Add(validClan);
                }
            }

            db.SaveChanges();
        }

        public void UpdateSyncMatch()
        {
            var validClans = db.ClanValidities.ToDictionary(l => l.Tag);
            var syncs = db.WarSyncs.OrderBy(w => w.Start).ToList();
            var wars = db.Wars.OrderBy(w => w.PreparationStartTime).ToList();

            if (syncs.Count() == 0 || validClans.Count() == 0 || wars.Count() == 0)
                return;

            var syncId = 0;
            var currentSync = syncs[syncId++];
            var allianceMatches = 0;
            var warMatches = 0;

            foreach (var war in wars)
            {
                var me = false;
                if(war.ClanTag == "#LGQJYLPY")
                    me = true;

                var clanIsValid = false;
                if (validClans.TryGetValue(war.ClanTag, out ClanValidity validClan))
                {
                    if (validClan.ValidFrom < war.PreparationStartTime && validClan.ValidTo > war.PreparationStartTime)
                        clanIsValid = true;
                }
                while(war.PreparationStartTime > currentSync.Finish && syncId < syncs.Count)
                {
                    currentSync.AllianceMatches = allianceMatches;
                    currentSync.WarMatches = warMatches;
                    currentSync = syncs[syncId++];
                    allianceMatches = 0;
                    warMatches = 0;
                }

                var matched = false;
                if (validClans.TryGetValue(war.OpponentTag, out validClan))
                {
                    var searchTime = war.PreparationStartTime;
                    if (validClan.ValidFrom < searchTime && validClan.ValidTo > searchTime)
                        matched = true;
                }
                war.Matched = matched;

                if (war.PreparationStartTime >= currentSync.Start && war.PreparationStartTime <= currentSync.Finish && (war.TeamSize == Constants.WAR_SIZE1 || war.TeamSize == Constants.WAR_SIZE2) && clanIsValid && !war.Friendly)
                {
                    war.Synced = true;
                    if (war.Matched)
                        allianceMatches++;
                    else
                        warMatches++;
                }
                else
                {
                    war.Synced = false;
                }
            }

            currentSync.AllianceMatches = allianceMatches;
            currentSync.WarMatches = warMatches;

            foreach(var sync in syncs)
            {
                var clanCount = 0;
                foreach(var validClan in validClans)
                {
                    if (validClan.Value.ValidFrom < sync.Start && validClan.Value.ValidTo > sync.Finish)
                        clanCount++;
                }
                sync.MissedStarts = clanCount - sync.AllianceMatches - sync.WarMatches;
                if (sync.AllianceMatches > 50 && sync.Finish < DateTime.UtcNow)
                    sync.Verified = true;
                else
                    sync.Verified = false;
            }

            db.SaveChanges();
        }

        public void UpdateClanStats()
        {
            var temp = (from w in db.Wars
                        join v in db.ClanValidities on w.ClanTag equals v.Tag
                        where w.PreparationStartTime > v.ValidFrom && w.PreparationStartTime < v.ValidTo
                        select new { w.ClanTag, w.Synced, w.Matched, w.Result }).ToList();

            var wars = (from w in temp
                        where w.Synced == true
                        group w by w.ClanTag into g
                        select new { Tag = g.Key, Count = g.Count() }).ToDictionary(w => w.Tag, w => w.Count);

            var wins = (from w in temp
                        where w.Synced == true && w.Result == "win"
                        group w by w.ClanTag into g
                        select new { Tag = g.Key, Count = g.Count() }).ToDictionary(w => w.Tag, w => w.Count);

            var matches = (from w in temp
                           where w.Synced == true && w.Matched == true
                           group w by w.ClanTag into g
                           select new { Tag = g.Key, Count = g.Count() }).ToDictionary(w => w.Tag, w => w.Count);

            var weights = new WeightCalculator(db).Calculate().ToDictionary(w => w.Tag);

            foreach( var clan in db.Clans )
            {
                if (wars.TryGetValue(clan.Tag, out int warCount))
                {
                    clan.WarCount = warCount;
                }
                else
                {
                    clan.WarCount = 0;
                    clan.WinPercentage = 0;
                    clan.MatchPercentage = 0;
                }

                if (clan.WarCount > 0)
                {
                    if (wins.TryGetValue(clan.Tag, out int winCount))
                        clan.WinPercentage = winCount * 100 / clan.WarCount;
                    else
                        clan.WinPercentage = 0;

                    if (matches.TryGetValue(clan.Tag, out int matchCount))
                        clan.MatchPercentage = matchCount * 100 / clan.WarCount;
                    else
                        clan.MatchPercentage = 0;
                }

                if (weights.TryGetValue(clan.Tag, out WeightCalculator.Results weight))
                {
                    clan.Th11Count = weight.Th11Count;
                    clan.Th10Count = weight.Th10Count;
                    clan.Th9Count = weight.Th9Count;
                    clan.Th8Count = weight.Th8Count;
                    clan.ThLowCount = weight.ThLowCount;
                    clan.EstimatedWeight = weight.EstimatedWeight;
                }
            }

            db.SaveChanges();
        }

        public void DeleteHistory()
        {
            var keepEventsSince = DateTime.UtcNow.AddDays(-1.0);

            db.Database.ExecuteSqlCommand("DELETE FROM ClanEvents WHERE EventDate < {0}", keepEventsSince);

            var keepAttacksSince = DateTime.UtcNow.AddDays(-7.0);

            //Don't remove half sync
            var isInMiddleSync2 = db.WarSyncs.Where(s => s.Start >= keepAttacksSince && s.Finish <= keepAttacksSince).FirstOrDefault();
            if (isInMiddleSync2 != null)
                keepAttacksSince = isInMiddleSync2.Start.AddHours(-1);

            db.Database.ExecuteSqlCommand("DELETE FROM WarMembers WHERE WarID IN ( SELECT ID FROM Wars WHERE EndTime < {0} )", keepAttacksSince);

            db.Database.ExecuteSqlCommand("DELETE FROM WarAttacks WHERE WarID IN ( SELECT ID FROM Wars WHERE EndTime < {0} )", keepAttacksSince);

            if (options.Value.Members > 0)
            {
                var keepMembersSince = DateTime.UtcNow.AddDays(-1.0 * options.Value.Members);

                db.Database.ExecuteSqlCommand("DELETE FROM PlayerEvents WHERE EventDate < {0}", keepMembersSince);

                var keepPlayersSince = DateTime.UtcNow.AddDays(-60);

                db.Database.ExecuteSqlCommand("DELETE FROM Players WHERE LastUpdated < {0}", keepPlayersSince);
            }

            if(options.Value.Wars > 0)
            {
                var keepWarsSince = DateTime.UtcNow.AddDays(-1.0 * options.Value.Wars);

                //Don't remove half sync
                var isInMiddleSync = db.WarSyncs.Where(s => s.Start >= keepWarsSince && s.Finish <= keepWarsSince).FirstOrDefault();
                if (isInMiddleSync != null)
                    keepWarsSince = isInMiddleSync.Start.AddHours(-1);

                db.Database.ExecuteSqlCommand("DELETE FROM WarSyncs WHERE Finish < {0}", keepWarsSince);

                db.Database.ExecuteSqlCommand("DELETE FROM Wars WHERE EndTime < {0}", keepWarsSince);

                db.Database.ExecuteSqlCommand("DELETE FROM ClanValidities WHERE ValidTo < {0}", keepWarsSince);
            }
        }

        private void AddSync(int year, int month, int day, int hour, int minute, int startOffset, int finishOffset)
        {
            var startDate = new DateTime(year, month, day, hour, minute, 0, DateTimeKind.Unspecified);
            var endDate = startDate.AddHours(2);

            var sync = db.WarSyncs.Where(s => s.Start == startDate && s.Finish == endDate).FirstOrDefault();
            if(sync != null)
            {
                sync.Verified = false;
                return;
            }

            //Convert endtime based sync to preparationstarttime based sync
            var searchOldStart = startDate.AddHours(47 - 1);
            var searchOldEnd = startDate.AddHours(47 + 4);
            var keepWarsSince = DateTime.UtcNow.AddDays(-1.0 * options.Value.Wars).AddHours(-47);
            if (endDate < keepWarsSince)
            {
                return;
            }

            sync = db.WarSyncs.Where(s => s.Start >= searchOldStart && s.Start <= searchOldEnd).FirstOrDefault();
            if(sync != null)
            {
                sync.Start = startDate;
                sync.Finish = endDate;
                sync.Verified = false;
            }
            else
            {
                sync = new WarSync { Start = startDate, Finish = endDate, Verified = false };
                db.WarSyncs.Add(sync);
            }

            db.SaveChanges();

            var searchStart = startDate.AddHours(47).AddMinutes(startOffset);
            var searchEnd = endDate.AddHours(47).AddMinutes(finishOffset);

            if (startOffset != 0 || finishOffset != 0)
            {
                var timeFormat = "yyyy'-'MM'-'dd' 'HH':'mm':'ss";

                if (startOffset != 0)
                {
                    var beforeSync = searchStart.AddHours(-4);
                    var afterSync = searchEnd.AddHours(4);

                    var sql = string.Format("UPDATE Wars SET preparationstarttime = datetime(endtime,'-47 hours','{0} minutes') WHERE endtime >= '{1}' AND endtime <= '{2}' AND starttime = '{3}'",
                            -1 * startOffset, beforeSync.ToString(timeFormat), afterSync.ToString(timeFormat), DateTime.MinValue.ToString(timeFormat));

                    db.Database.ExecuteSqlCommand(sql);
                }

                if (finishOffset > startOffset)
                {
                    var beforeSync = searchEnd;
                    var afterSync = searchEnd.AddHours(4);

                    var sql = string.Format("UPDATE Wars SET preparationstarttime = datetime(endtime,'-47 hours','{0} minutes') WHERE endtime >= '{1}' AND endtime <= '{2}' AND starttime = '{3}'",
                            -1 * finishOffset, beforeSync.ToString(timeFormat), afterSync.ToString(timeFormat), DateTime.MinValue.ToString(timeFormat));

                    db.Database.ExecuteSqlCommand(sql);
                }
            }
        }

        private void FillSyncsUtc()
        {
            db.Database.ExecuteSqlCommand("UPDATE Wars SET preparationstarttime = datetime(endtime,'-47 hours') WHERE preparationstarttime = '0001-01-01 00:00:00'");

            db.Database.ExecuteSqlCommand("UPDATE WarSyncs SET Verified = 1");

            AddSync(2017, 10, 21, 16, 0, 15, 15);
            AddSync(2017, 10, 23, 16, 50, 0, 0);
            AddSync(2017, 10, 25, 18, 0, 0, 0);
            AddSync(2017, 10, 27, 18, 56, 0, 0);
            AddSync(2017, 10, 29, 21, 19, 0, 0);
            AddSync(2017, 10, 31, 22, 36, 20, 20);
            AddSync(2017, 11, 2, 23, 47, 0, 0);
            AddSync(2017, 11, 5, 0, 58, 0, 0);
            AddSync(2017, 11, 7, 3, 7, 0, 0);
            AddSync(2017, 11, 9, 7, 25, 0, 0);
            AddSync(2017, 11, 11, 9, 4, 0, 0);
            AddSync(2017, 11, 13, 10, 4, 0, 0);
            AddSync(2017, 11, 15, 11, 4, 27, 27);
            AddSync(2017, 11, 17, 15, 55, 0, 0);
            AddSync(2017, 11, 19, 17, 0, 0, 0);
            AddSync(2017, 11, 21, 17, 52, 0, 0);
            AddSync(2017, 11, 23, 18, 43, 0, 0);
            AddSync(2017, 11, 25, 19, 25, 20, 20);
            AddSync(2017, 11, 27, 20, 44, 15, 15);
            AddSync(2017, 11, 29, 21, 55, 0, 0);
            AddSync(2017, 12, 1, 22, 54, 0, 0);
            AddSync(2017, 12, 3, 23, 56, 13, 13);
            AddSync(2017, 12, 6, 9, 27, 0, 0);
            AddSync(2017, 12, 8, 13, 55, 0, 0);
            AddSync(2017, 12, 10, 14, 23, 0, 0);
            AddSync(2017, 12, 12, 18, 40, 0, 0);
            AddSync(2017, 12, 14, 19, 42, 0, 0);
            AddSync(2017, 12, 16, 20, 55, 110, 110);
            AddSync(2017, 12, 18, 23, 15, 0, 0);
            AddSync(2017, 12, 20, 23, 45, 15, 15);
            AddSync(2017, 12, 23, 0, 45, 0, 0);
            AddSync(2017, 12, 25, 3, 49, 0, 0);
            AddSync(2017, 12, 27, 5, 0, 0, 0);
            AddSync(2017, 12, 29, 7, 12, 0, 0);
            AddSync(2017, 12, 31, 8, 30, 0, 0);
            AddSync(2018, 1, 2, 10, 26, 0, 0);
            AddSync(2018, 1, 4, 12, 50, 0, 0);
            AddSync(2018, 1, 6, 14, 0, 0, 0);
            AddSync(2018, 1, 8, 19, 0, 0, 0);
            AddSync(2018, 1, 10, 19, 50, 0, 0);
            AddSync(2018, 1, 12, 20, 43, 0, 0);
            AddSync(2018, 1, 14, 21, 40, 0, 0);
            AddSync(2018, 1, 16, 22, 30, 0, 0);
            AddSync(2018, 1, 18, 23, 35, 0, 0);
            AddSync(2018, 1, 21, 1, 5, 0, 0);
            AddSync(2018, 1, 23, 3, 5, 28, 28);
            AddSync(2018, 1, 25, 5, 0, 0, 0);
            AddSync(2018, 1, 27, 10, 50, 0, 27);
            AddSync(2018, 1, 29, 12, 18, 0, 0);
            AddSync(2018, 1, 31, 15, 18, 0, 0);
            AddSync(2018, 2, 2, 16, 20, 0, 0);
            AddSync(2018, 2, 4, 17, 15, 0, 0);
            AddSync(2018, 2, 6, 18, 08, 20, 20);
            AddSync(2018, 2, 8, 19, 1, 0, 0);
            AddSync(2018, 2, 10, 20, 14, 0, 0);
            AddSync(2018, 2, 12, 21, 0, 25, 25);
            AddSync(2018, 2, 14, 22, 43, 0, 0);
            AddSync(2018, 2, 16, 23, 40, 0, 0);
            AddSync(2018, 2, 19, 6, 35, 0, 0);
            AddSync(2018, 2, 21, 8, 15, 0, 0);
            AddSync(2018, 2, 23, 10, 50, 0, 0);
            AddSync(2018, 2, 25, 12, 55, 32, 44);
            AddSync(2018, 2, 27, 15, 40, 0, 0);
            AddSync(2018, 3, 1, 17, 30, 0, 0);
            AddSync(2018, 3, 3, 18, 20, 90, 90);
            AddSync(2018, 3, 5, 22, 0, 0, 0);
            AddSync(2018, 3, 8, 0, 30, 0, 0);
            AddSync(2018, 3, 10, 3, 20, 0, 0);
            AddSync(2018, 3, 12, 5, 40, 0, 0);
            AddSync(2018, 3, 14, 7, 14, 0, 0);
            AddSync(2018, 3, 16, 9, 15, -8, -8);
            AddSync(2018, 3, 18, 11, 20, 0, 0);
            AddSync(2018, 3, 20, 13, 0, 0, 0);
            AddSync(2018, 3, 22, 14, 40, 0, 0);
            AddSync(2018, 3, 24, 18, 0, 0, 0);
            AddSync(2018, 3, 26, 20, 5, 0, 0);
            AddSync(2018, 3, 28, 21, 30, 0, 0);
            AddSync(2018, 3, 30, 22, 50, 0, 0);
            AddSync(2018, 4, 2, 1, 35, 0, 0);
            AddSync(2018, 4, 4, 6, 0, 0, 0);
            AddSync(2018, 4, 6, 8, 0, 0, 0);
            AddSync(2018, 4, 8, 21, 30, 105, 105);
            AddSync(2018, 4, 11, 0, 45, 0, 0);
            AddSync(2018, 4, 13, 2, 27, 30, 30);
            AddSync(2018, 4, 15, 4, 3, 0, 0);
            AddSync(2018, 4, 17, 5, 4, 0, 0);
            AddSync(2018, 4, 19, 7, 1, 0, 0);
            AddSync(2018, 4, 21, 19, 56, 0, 0);
            AddSync(2018, 4, 23, 21, 20, 24, 24);
            AddSync(2018, 4, 25, 22, 50, 0, 0);
            AddSync(2018, 4, 28, 0, 10, 0, 0);
            AddSync(2018, 4, 30, 1, 55, 0, 0);
            AddSync(2018, 5, 2, 8, 16, 0, 0);
            AddSync(2018, 5, 4, 9, 30, 0, 0);
            AddSync(2018, 5, 6, 11, 15, 0, 0);
            AddSync(2018, 5, 8, 12, 30, 0, 0);
            AddSync(2018, 5, 10, 14, 40, 0, 0);
            AddSync(2018, 5, 12, 15, 55, 0, 0);
            AddSync(2018, 5, 14, 17, 30, 0, 0);
            AddSync(2018, 5, 16, 19, 0, 60, 60);
            AddSync(2018, 5, 18, 20, 20, 0, 0);
            AddSync(2018, 5, 20, 21, 55, 0, 0);
            AddSync(2018, 5, 22, 23, 0, 0, 0);
            AddSync(2018, 5, 25, 0, 30, 0, 0);
            AddSync(2018, 5, 27, 1, 50, 0, 0);

            db.SaveChanges();

            db.Database.ExecuteSqlCommand("DELETE FROM WarSyncs WHERE Verified = 1");
        }
    }
}
