using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using Microsoft.EntityFrameworkCore;
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
            foreach (var s in db.WarSyncs.ToList())
            {
                db.WarSyncs.Remove(s);
            }
            db.SaveChanges();

            var syncs = new List<WarSync>();

            var dateQ = new Queue<DateTime>();
            for (int i = 0; i < 10; i++)
                dateQ.Enqueue(DateTime.MinValue);

            var oneHour = new TimeSpan(1, 0, 0);
            var twoHour = new TimeSpan(2, 0, 0);

            var sync = new WarSync();

            var q = from w in db.Wars orderby w.EndTime select w.EndTime;
            foreach (var endTime in q)
            {
                var fewWarsBeforeStartedAt = dateQ.Dequeue();
                var fewWarsStartetWithin = endTime.Subtract(fewWarsBeforeStartedAt);

                if (!sync.IsStarted)
                {
                    if (fewWarsStartetWithin < oneHour)
                    {
                        sync.Start = fewWarsBeforeStartedAt;
                    }
                }
                else
                {
                    if (fewWarsStartetWithin > twoHour)
                    {
                        //This will be fixed later, finish would be last value in queue
                        //TODO: test if dateQ.Last() would work
                        sync.Finish = fewWarsBeforeStartedAt.Add(twoHour);
                        syncs.Add(sync);
                        sync = new WarSync();
                    }
                }

                dateQ.Enqueue(endTime);
            }

            //Latest sync is still active
            if (sync.IsStarted && !sync.IsFinished)
            {
                sync.Finish = dateQ.Peek().Add(twoHour);
                syncs.Add(sync);
            }

            var clanList = (from c in db.ClanValidities select c).ToDictionary(c => c.Tag);

            foreach (var s in syncs)
            {
                var searchTime = s.SearchTime;
                var latestWarStarted = s.Start;

                var warQ = from w in db.Wars
                           where w.EndTime >= s.Start && w.EndTime <= s.Finish
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

                    ClanValidity clan;
                    if (clanList.TryGetValue(res.ClanTag, out clan))
                    {
                        if (clan.ValidFrom < searchTime && clan.ValidTo > searchTime)
                        {
                            ClanValidity opponent;
                            if (clanList.TryGetValue(res.OpponentTag, out opponent))
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

                db.WarSyncs.Add(s);
            }

            db.SaveChanges();
        }

        public void UpdateValidities()
        {
            var currentClans = db.Clans.ToDictionary(c => c.Tag);
            var validClans = db.ClanValidities.ToDictionary(l => l.Tag);

            var firstMatches = (from w in db.Wars
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
                        db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    if (firstMatches.TryGetValue(clan.Key, out firstMatch))
                    {
                        var firstSearch = firstMatch.AddDays(-2);
                        if (validClan.ValidFrom > firstSearch)
                        {
                            validClan.ValidFrom = firstSearch;
                            db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    if (validClan.Group != currentClan.Group)
                    {
                        validClan.Group = currentClan.Group;
                        db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
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

            var currentSync = syncs.First();
            
            foreach(var war in db.Wars)
            {
                var warModified = false;
                var clanIsValid = false;
                ClanValidity validClan;
                if (validClans.TryGetValue(war.ClanTag, out validClan))
                {
                    if (validClan.ValidFrom < war.SearchTime && validClan.ValidTo > war.SearchTime)
                        clanIsValid = true;
                }
                if(war.EndTime < currentSync.Start || war.EndTime > currentSync.Finish)
                {
                    foreach (var sync in syncs.Where(s => war.EndTime >= s.Start && war.EndTime <= s.Finish))
                    {
                        currentSync = sync;
                        break;
                    }
                }

                if(war.EndTime >= currentSync.Start && war.EndTime <= currentSync.Finish && war.TeamSize == 40 && clanIsValid)
                {
                    if(!war.Synced)
                    {
                        war.Synced = true;
                        warModified = true;
                    }
                }
                else
                {
                    if (war.Synced)
                    {
                        war.Synced = false;
                        warModified = true;
                    }
                }

                var matched = false;
                if(validClans.TryGetValue(war.OpponentTag, out validClan))
                {
                    var searchTime = war.SearchTime;
                    if (validClan.ValidFrom < searchTime && validClan.ValidTo > searchTime)
                        matched = true;
                }

                if(war.Matched != matched)
                {
                    war.Matched = matched;
                    warModified = true;
                }

                if(warModified)
                {
                    db.Update(war);
                }
            }

            db.SaveChanges();

        }

        public void UpdateClanStats()
        {
            var temp = (from w in db.Wars.Select(w => new { w.ClanTag, w.Synced, w.Matched, w.Result }) select w).ToList();

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
                int warCount, winCount, matchCount;

                if (wars.TryGetValue(clan.Tag, out warCount))
                    clan.WarCount = warCount;

                if (clan.WarCount > 0)
                {
                    if (wins.TryGetValue(clan.Tag, out winCount))
                        clan.WinPercentage = winCount * 100 / clan.WarCount;
                    if (matches.TryGetValue(clan.Tag, out matchCount))
                        clan.MatchPercentage = matchCount * 100 / clan.WarCount;
                }

                WeightCalculator.Results weight = null;
                if (weights.TryGetValue(clan.Tag, out weight))
                {
                    clan.Th11Count = weight.Th11Count;
                    clan.Th10Count = weight.Th10Count;
                    clan.Th9Count = weight.Th9Count;
                    clan.Th8Count = weight.Th8Count;
                    clan.ThLowCount = weight.ThLowCount;
                    clan.EstimatedWeight = weight.EstimatedWeight;
                }

                db.Update(clan);
            }

            db.SaveChanges();
        }

        public void DeleteHistory()
        {
            var MAX_UPDATES = 1000;

            if(history.Value.Members > 0)
            {
                var keepMembersSince = DateTime.UtcNow.AddDays(-1.0 * history.Value.Members);

                var historyEvents = db.PlayerEvents.Where(e => e.EventDate < keepMembersSince).OrderBy(e => e.EventDate).Take(MAX_UPDATES);
                db.PlayerEvents.RemoveRange(historyEvents);
                db.SaveChanges();

                var historyPlayers = db.Players.Where(p => p.LastUpdated < keepMembersSince).OrderBy(p => p.LastUpdated).Take(MAX_UPDATES);
                db.Players.RemoveRange(historyPlayers);
                db.SaveChanges();

                var clanEvents = = db.ClanEvents.Where(e => e.EventDate < keepMembersSince).OrderBy(e => e.EventDate).Take(MAX_UPDATES);
                db.ClanEvents.RemoveRange(clanEvents);
                db.SaveChanges();
            }

            if(history.Value.Wars > 0)
            {
                var keepWarsSince = DateTime.UtcNow.AddDays(-1.0 * history.Value.Wars);

                //Don't remove half sync
                var isInMiddleSync = db.WarSyncs.Where(s => s.Start >= keepWarsSince && s.Finish <= keepWarsSince).FirstOrDefault();
                if (isInMiddleSync != null)
                    keepWarsSince = isInMiddleSync.Start.AddHours(-1);

                var historySyncs = db.WarSyncs.Where(s => s.Finish < keepWarsSince);
                db.WarSyncs.RemoveRange(historySyncs);
                db.SaveChanges();

                var historyWars = db.Wars.Where(w => w.EndTime < keepWarsSince).OrderBy(w => w.EndTime).Take(MAX_UPDATES);
                db.Wars.RemoveRange(historyWars);
                db.SaveChanges();

                var historyValidities = db.ClanValidities.Where(v => v.ValidTo < keepWarsSince);
                db.ClanValidities.RemoveRange(historyValidities);
                db.SaveChanges();
            }
        }
    }
}
