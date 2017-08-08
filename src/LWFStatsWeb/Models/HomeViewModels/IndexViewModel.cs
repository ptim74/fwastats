using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.HomeViewModels
{
    public class IndexViewModel
    {
        public ICollection<SyncStats> LastSyncs { get; set; }
        public ICollection<SyncHistory> SyncHistories { get; set; }
        public ICollection<CounterStats> Counters { get; set; }
        public ICollection<TownhallCounter> TownhallCounters { get; set; }
    }

    public class SyncStats
    {
        public string Name { get; set; }
        public string Status { get; set; }
        public int AllianceMatches { get; set; }
        public int WarMatches { get; set; }
        public int NotStarted { get; set; }
    }

    public class SyncHistory
    {
        public ICollection<SyncStats> Syncs { get; set; }
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

    public class TownhallCounter
    {
        public int Clans { get; set; }
        public int Weight { get; set; }
        public double TH11 { get; set; }
        public double TH10 { get; set; }
        public double TH9 { get; set; }
        public double TH8 { get; set; }
    }
}
