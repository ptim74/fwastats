using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.ClanViewModels
{
    public class ClanIndexClan
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public string BadgeUrl { get; set; }
        public int WarCount { get; set; }
        public double MatchPercentage { get; set; }
        public double WinPercentage { get; set; }

        public int Th14Count { get; set; }
        public int Th13Count { get; set; }
        public int Th12Count { get; set; }
        public int Th11Count { get; set; }
        public int Th10Count { get; set; }
        public int Th9Count { get; set; }
        public int Th8Count { get; set; }
        public int ThLowCount { get; set; }

        public int EstimatedWeight { get; set; }

        public SubmitRestriction SubmitRestriction { get; set; }

        public DateTime WeightSubmitDate { get; set; }

        public bool PendingWeightSubmit { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
