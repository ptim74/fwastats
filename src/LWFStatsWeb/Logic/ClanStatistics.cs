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

            var clanList = (from c in db.Clans select c.Tag).ToList();

            foreach (var s in syncs)
            {
                var latestWarStarted = s.Start;

                var warQ = from w in db.Wars
                           join o in db.WarOpponents on w.ID equals o.WarID
                           where w.EndTime >= s.Start && w.EndTime <= s.Finish
                           select new { EndTime = w.EndTime, OpponentTag = o.Tag };

                foreach (var res in warQ)
                {
                    if (res.EndTime > latestWarStarted)
                        latestWarStarted = res.EndTime;
                    if (clanList.Contains(res.OpponentTag))
                        s.AllianceMatches++;
                    else
                        s.WarMatches++;
                }

                s.Finish = latestWarStarted;
                s.MissedStarts = clanList.Count - s.AllianceMatches - s.WarMatches;

                db.WarSyncs.Add(s);
            }

            db.SaveChanges();
        }
    }
}
