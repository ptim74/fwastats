using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Logic
{
    public class WeightResultOptions : List<WeightResultDetails>
    {
        public WeightResultDetails SelectTeamSize(int teamSize)
        {
            return this.Single(i => i.TeamSize == teamSize);
        }
    }

    public class WeightResultDetails
    {
        public int TeamSize { get; set; }
        public string SheetId { get; set; }
        public string ResultRange { get; set; }
        public string PendingRange { get; set; }
    }
}
