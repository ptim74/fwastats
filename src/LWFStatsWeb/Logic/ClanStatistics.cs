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
            var halfHour = new TimeSpan(0, 30, 0);

            var sync = new WarSync();

            var q = from w in db.Wars orderby w.EndTime select w.EndTime;
            foreach (var endTime in q)
            {
                var fewWarsBeforeStartedAt = dateQ.Dequeue();
                var fewWarsStartetWithin = endTime.Subtract(fewWarsBeforeStartedAt);

                if (!sync.IsStarted)
                {
                    if (fewWarsStartetWithin < halfHour)
                    {
                        sync.Start = fewWarsBeforeStartedAt;
                    }
                }
                else
                {
                    if (fewWarsStartetWithin > oneHour)
                    {
                        //This will be fixed later, finish would be last value in queue
                        //TODO: test if dateQ.Last() would work
                        sync.Finish = fewWarsBeforeStartedAt.Add(oneHour);
                        syncs.Add(sync);
                        sync = new WarSync();
                    }
                }

                dateQ.Enqueue(endTime);
            }

            //Latest sync is still active
            if (sync.IsStarted && !sync.IsFinished)
            {
                sync.Finish = dateQ.Peek().Add(oneHour);
                syncs.Add(sync);
            }

            var clanList = (from c in db.ClanValidities select c).ToDictionary(c => c.Tag);

            foreach (var s in syncs)
            {
                var searchTime = s.Start.AddHours(-47);
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
                    if (validClan.ValidTo > DateTime.Now)
                    {
                        validClan.ValidTo = DateTime.Now;
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
                    var validClan = new ClanValidity() { Tag = clan, Name = currentClan.Name, ValidFrom = DateTime.Now, ValidTo = DateTime.MaxValue };
                    if (firstMatches.ContainsKey(clan))
                        validClan.ValidFrom = firstMatches[clan].MinEndTime.AddDays(-1);
                    db.ClanValidities.Add(validClan);
                }
                else
                {
                    var validClan = validClans[clan];
                    if(validClan.ValidTo < DateTime.Now)
                    {
                        validClan.ValidTo = DateTime.MaxValue;
                        db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                    }
                    if (firstMatches.ContainsKey(clan))
                    {
                        if (validClan.ValidFrom > firstMatches[clan].MinEndTime)
                        {
                            validClan.ValidFrom = firstMatches[clan].MinEndTime;
                            db.Entry(validClan).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                        }
                    }
                }
            }

            db.SaveChanges();
        }
    }
}
