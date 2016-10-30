using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.HomeViewModels
{
    public class IndexViewModel
    {
        public List<SyncStats> LastSyncs { get; set; }
        public List<SyncHistory> SyncHistories { get; set; }
        public List<CounterStats> Counters { get; set; }
    }

    public class SyncStats
    {
        public string Name { get; set; }
        public string LastSyncName { get; set; }
        public int AllianceMatches { get; set; }
        public int WarMatches { get; set; }
        public int NotStarted { get; set; }
    }

    public class SyncHistory
    {
        public List <SyncStats> Syncs { get; set; }
    }

    public class CounterStats
    {
        public int ClanCount { get; set; }
        public int MemberCount { get; set; }
        public double MatchPercentage { get; set; }
        public double WinPercentage { get; set; }
    }

    public class ClanDetails
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public string BadgeUrl { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
