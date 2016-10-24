using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class ClanIndexClan
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public string BadgeUrl { get; set; }
        public string Group { get; set; }
        public int WarCount { get; set; }
        public double MatchPercentage { get; set; }
        public double WinPercentage { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
