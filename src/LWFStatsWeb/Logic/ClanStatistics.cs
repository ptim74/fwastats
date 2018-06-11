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
                    clan.Th12Count = weight.Th12Count;
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
    }
}
