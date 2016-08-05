using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.HomeViewModels
{
    public class IndexViewModel
    {
        public SyncStats LatestSync { get; set; }
        public CounterStats Counters { get; set; }
        public List<ClanDetails> ClansNeedingHelp { get; set; }
    }

    public class SyncStats
    {
        public string Name { get; set; }
        public int AllianceMatches { get; set; }
        public int WarMatches { get; set; }
        public int NotStarted { get; set; }
    }

    public class CounterStats
    {
        public int ClanCount { get; set; }
        public int MemberCount { get; set; }
        public int WarCount { get; set; }
        public int SyncCount { get; set; }
    }

    public class ClanDetails
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public string BadgeURL { get; set; }

        public string LinkID
        {
            get
            {
                return Tag.Replace("#", "");
            }
        }
    }
}
