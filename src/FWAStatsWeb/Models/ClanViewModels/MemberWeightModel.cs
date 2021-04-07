using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class MemberWeightModel
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Weight { get; set; }
        public bool InWar { get; set; }
        public int TownHallLevel { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
