using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public interface IClanStatistics
    {
        void CalculateSyncs();
        void UpdateValidities();
        void UpdateSyncMatch();
    }

    public class ClanStatistics : IClanStatistics
    {
        private readonly ApplicationDbContext db;

        public ClanStatistics(ApplicationDbContext db)
        {
            this.db = db;
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
                    if (clanList.ContainsKey(res.ClanTag))
                    {
                        var clan = clanList[res.ClanTag];
                        if (clan.ValidFrom < searchTime && clan.ValidTo > searchTime)
                        {
                            if (clanList.ContainsKey(res.OpponentTag))
                            {
                                var opponent = clanList[res.OpponentTag];
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
                                select new { Tag = g.Key.OpponentTag, MinEndTime = g.Min(w => w.EndTime) }).ToDictionary(d => d.Tag);

            //Deleted clans
            foreach (var clan in validClans.Keys)
            {
                if(!currentClans.Keys.Contains(clan))
                {
                    var validClan = validClans[clan];
                    if (validClan.ValidTo > DateTime.UtcNow)
                    {
                        validClan.ValidTo = DateTime.UtcNow;
                        db.ClanValidities.Update(validClan);
                    }
                }
            }

            //New or existing clans
            foreach(var clan in currentClans.Keys)
            {
                var currentClan = currentClans[clan];
                if(!validClans.Keys.Contains(clan))
                {
                    var validClan = new ClanValidity() { Tag = clan, Name = currentClan.Name, ValidFrom = DateTime.UtcNow, ValidTo = DateTime.MaxValue, Group = currentClan.Group };
                    if (firstMatches.ContainsKey(clan))
                        validClan.ValidFrom = firstMatches[clan].MinEndTime.AddDays(-2);
                    db.ClanValidities.Add(validClan);
                }
                else
                {
                    var validClan = validClans[clan];
                    if(validClan.ValidTo < DateTime.UtcNow)
                    {
                        validClan.ValidTo = DateTime.MaxValue;
                        db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    if (firstMatches.ContainsKey(clan))
                    {
                        var firstSearch = firstMatches[clan].MinEndTime.AddDays(-2);
                        if (validClan.ValidFrom > firstSearch)
                        {
                            validClan.ValidFrom = firstSearch;
                            db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                    if(validClan.Group != currentClan.Group)
                    {
                        validClan.Group = currentClan.Group;
                        db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
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

                if(war.EndTime < currentSync.Start || war.EndTime > currentSync.Finish)
                {
                    foreach(var sync in syncs)
                    {
                        if(war.EndTime >= sync.Start && war.EndTime <= sync.Finish)
                        {
                            currentSync = sync;
                            break;
                        }
                    }
                }

                if(war.EndTime >= currentSync.Start && war.EndTime <= currentSync.Finish)
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
                if(validClans.ContainsKey(war.OpponentTag))
                {
                    var opponent = validClans[war.OpponentTag];
                    var searchTime = war.SearchTime;
                    if (opponent.ValidFrom < searchTime && opponent.ValidTo > searchTime)
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
    }
}
