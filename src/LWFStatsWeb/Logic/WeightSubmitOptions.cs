using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class WeightSubmitOptions : List<WeightSubmitDetails>
    {
        public WeightSubmitDetails SelectTeamSize(int teamSize)
        {
            return this.Single(i => i.TeamSize == teamSize);
        }
    }

    public class WeightSubmitDetails
    {
        public int TeamSize { get; set; }
        public string SubmitURL { get; set; }
        public string ResponseURL { get; set; }
        public string SheetId { get; set; }
        public string TabName { get; set; }
        public string ClanNameRange { get; set; }
        public string CompositionRange { get; set; }
        public string WeightRange { get; set; }
        public string TagRange { get; set; }
        public string THRange { get; set; }
    }
}
