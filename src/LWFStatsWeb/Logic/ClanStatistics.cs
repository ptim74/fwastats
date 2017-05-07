﻿using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class StatisicsHistory
    {
        public int Wars { get; set; }
        public int Members { get; set; }
    }

    public interface IClanStatistics
    {
        void DeleteHistory();
        void UpdateValidities();
        void CalculateSyncs();
        void UpdateSyncMatch();
        void UpdateClanStats();
    }

    public class ClanStatistics : IClanStatistics
    {
        private readonly ApplicationDbContext db;
        private readonly IOptions<StatisicsHistory> history;

        public ClanStatistics(
            ApplicationDbContext db,
            IOptions<StatisicsHistory> history
            )
        {
            this.db = db;
            this.history = history;
        }

        public void CalculateSyncs()
        {
            var syncs = new List<WarSync>();

            var dateQ = new Queue<DateTime>();
            for (int i = 0; i < 10; i++)
                dateQ.Enqueue(DateTime.MinValue);

            var enterSync = new TimeSpan(0, 15, 0);
            var exitSync = new TimeSpan(1, 0, 0);
            var syncDuration = new TimeSpan(2, 15, 0);

            var sync = new WarSync();

            var q = from w in db.Wars where w.Friendly == false orderby w.EndTime select w.EndTime;
            foreach (var endTime in q)
            {
                var fewWarsBeforeStartedAt = dateQ.Dequeue();
                var fewWarsStartetWithin = endTime.Subtract(fewWarsBeforeStartedAt);

                if (!sync.IsStarted)
                {
                    if (fewWarsStartetWithin < enterSync)
                    {
                        sync.Start = fewWarsBeforeStartedAt;
                    }
                }
                else
                {
                    if (fewWarsStartetWithin > exitSync)
                    {
                        //This will be fixed later, finish would be last value in queue
                        //TODO: test if dateQ.Last() would work
                        //sync.Finish = fewWarsBeforeStartedAt.Add(exitSync);
                        sync.Finish = sync.Start.Add(syncDuration);
                        syncs.Add(sync);
                        sync = new WarSync();
                    }
                }

                dateQ.Enqueue(endTime);
            }

            //Latest sync is still active
            if (sync.IsStarted && !sync.IsFinished)
            {
                //sync.Finish = dateQ.Peek().Add(exitSync);
                sync.Finish = sync.Start.Add(syncDuration);
                syncs.Add(sync);
            }

            var clanList = (from c in db.ClanValidities select c).ToDictionary(c => c.Tag);

            foreach (var s in syncs)
            {
                var searchTime = s.SearchTime;
                var latestWarStarted = s.Start;

                var warQ = from w in db.Wars
                           where w.EndTime >= s.Start && w.EndTime <= s.Finish && w.Friendly == false
                           select new { EndTime = w.EndTime, ClanTag = w.ClanTag, OpponentTag = w.OpponentTag };

                s.MissedStarts = 0;
                foreach (var clan in clanList.Values)
                {
                    if (clan.ValidFrom < searchTime && clan.ValidTo > searchTime)
                        s.MissedStarts++;
                }

                foreach (var res in warQ)
                {
                    if (res.EndTime > latestWarStarted)
                        latestWarStarted = res.EndTime;

                    if (clanList.TryGetValue(res.ClanTag, out ClanValidity clan))
                    {
                        if (clan.ValidFrom < searchTime && clan.ValidTo > searchTime)
                        {
                            if (clanList.TryGetValue(res.OpponentTag, out ClanValidity opponent))
                            {
                                if (opponent.ValidFrom < searchTime && opponent.ValidTo > searchTime)
                                {
                                    s.AllianceMatches++;
                                    s.MissedStarts--;
                                }
                                else
                                {
                                    s.WarMatches++;
                                    s.MissedStarts--;
                                }
                            }
                            else
                            {
                                s.WarMatches++;
                                s.MissedStarts--;
                            }
                        }
                    }
                }

                s.Finish = latestWarStarted;
            }

            var syncId = 0;
            var newSync = syncs[syncId++];

            foreach(var existingSync in db.WarSyncs.OrderBy(s => s.Start))
            {
                if (newSync != null)
                {
                    if (existingSync.Finish > newSync.Start && existingSync.Start < newSync.Finish)
                    {
                        existingSync.Start = newSync.Start;
                        existingSync.Finish = newSync.Finish;
                        existingSync.MissedStarts = newSync.MissedStarts;
                        existingSync.AllianceMatches = newSync.AllianceMatches;
                        existingSync.WarMatches = newSync.WarMatches;
                        if (syncId < syncs.Count)
                            newSync = syncs[syncId++];
                        else
                            newSync = null;
                    }
                    else
                    {
                        db.WarSyncs.Remove(existingSync);
                        while (newSync.Finish < existingSync.Start && syncId < syncs.Count)
                        {
                            db.WarSyncs.Add(newSync);
                            if (syncId < syncs.Count)
                                newSync = syncs[syncId++];
                            else
                                newSync = null;
                        }
                    }
                }
            }

            if (newSync != null) //This was never added in loop
                syncId--;

            while (syncId < syncs.Count)
            {
                db.WarSyncs.Add(syncs[syncId++]);
            }

            db.SaveChanges();
        }

        public void UpdateValidities()
        {
            var currentClans = db.Clans.ToDictionary(c => c.Tag);
            var validClans = db.ClanValidities.ToDictionary(l => l.Tag);

            var firstMatches = (from w in db.Wars where w.Friendly == false
                                group w by new { w.OpponentTag } into g
                                select new { Tag = g.Key.OpponentTag, MinEndTime = g.Min(w => w.EndTime) }).ToDictionary(d => d.Tag, d => d.MinEndTime);

            //Deleted clans
            foreach (var clan in validClans)
            {
                if(!currentClans.ContainsKey(clan.Key))
                {
                    var validClan = clan.Value;
                    if (validClan.ValidTo > DateTime.UtcNow)
                    {
                        validClan.ValidTo = DateTime.UtcNow;
                        db.ClanValidities.Update(validClan);
                    }
                }
            }

            //New or existing clans
            foreach(var clan in currentClans)
            {
                var currentClan = clan.Value;
                ClanValidity validClan;
                DateTime firstMatch;
                if (validClans.TryGetValue(clan.Key, out validClan))
                {
                    if (validClan.ValidTo < DateTime.UtcNow)
                    {
                        validClan.ValidTo = DateTime.MaxValue;
                        db.Entry(validClan).State = EntityState.Modified;
                    }
                    if (firstMatches.TryGetValue(clan.Key, out firstMatch))
                    {
                        var firstSearch = firstMatch.AddDays(-2);
                        if (validClan.ValidFrom > firstSearch)
                        {
                            validClan.ValidFrom = firstSearch;
                            db.Entry(validClan).State = EntityState.Modified;
                        }
                    }
                    if (validClan.Group != currentClan.Group)
                    {
                        validClan.Group = currentClan.Group;
                        db.Entry(validClan).State = EntityState.Modified;
                    }
                }
                else
                {
                    validClan = new ClanValidity() { Tag = clan.Key, Name = currentClan.Name, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.MaxValue, Group = currentClan.Group };
                    if (firstMatches.TryGetValue(clan.Key, out firstMatch))
                        validClan.ValidFrom = firstMatch.AddDays(-2);
                    db.ClanValidities.Add(validClan);
                }
            }

            db.SaveChanges();
        }

        public void UpdateSyncMatch()
        {
            var validClans = db.ClanValidities.ToDictionary(l => l.Tag);
            var syncs = db.WarSyncs.OrderBy(w => w.Start).ToList();
            var wars = db.Wars.OrderBy(w => w.EndTime).ToList();

            if (syncs.Count() == 0 || validClans.Count() == 0 || wars.Count() == 0)
                return;

            var syncId = 0;
            var currentSync = syncs[syncId++];

            foreach (var war in wars)
            {
                var clanIsValid = false;
                if (validClans.TryGetValue(war.ClanTag, out ClanValidity validClan))
                {
                    if (validClan.ValidFrom < war.SearchTime && validClan.ValidTo > war.SearchTime)
                        clanIsValid = true;
                }
                while(war.EndTime > currentSync.Finish && syncId < syncs.Count)
                {
                    currentSync = syncs[syncId++];
                }

                if(war.EndTime >= currentSync.Start && war.EndTime <= currentSync.Finish && war.TeamSize == 40 && clanIsValid)
                {
                    war.Synced = true;
                }
                else
                {
                    war.Synced = false;
                }

                var matched = false;
                if(validClans.TryGetValue(war.OpponentTag, out validClan))
                {
                    var searchTime = war.SearchTime;
                    if (validClan.ValidFrom < searchTime && validClan.ValidTo > searchTime)
                        matched = true;
                }

                war.Matched = matched;
            }

            db.SaveChanges();
        }

        public void UpdateClanStats()
        {
            var temp = (from w in db.Wars.Where(w => w.Friendly == false).Select(w => new { w.ClanTag, w.Synced, w.Matched, w.Result }) select w).ToList();

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
                    clan.WarCount = warCount;

                if (clan.WarCount > 0)
                {
                    if (wins.TryGetValue(clan.Tag, out int winCount))
                        clan.WinPercentage = winCount * 100 / clan.WarCount;
                    if (matches.TryGetValue(clan.Tag, out int matchCount))
                        clan.MatchPercentage = matchCount * 100 / clan.WarCount;
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

            if (history.Value.Members > 0)
            {
                var keepMembersSince = DateTime.UtcNow.AddDays(-1.0 * history.Value.Members);

                db.Database.ExecuteSqlCommand("DELETE FROM PlayerEvents WHERE EventDate < {0}", keepMembersSince);

                db.Database.ExecuteSqlCommand("DELETE FROM Players WHERE LastUpdated < {0}", keepMembersSince);
            }

            if(history.Value.Wars > 0)
            {
                var keepWarsSince = DateTime.UtcNow.AddDays(-1.0 * history.Value.Wars);

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
