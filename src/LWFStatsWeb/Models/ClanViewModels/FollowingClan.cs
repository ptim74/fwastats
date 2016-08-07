using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class FollowingClan
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string BadgeURL { get; set; }
        public int Wars { get; set; }
        public string LatestClan { get; set; }
        public string LatestTag { get; set; }
        public string LatestDate { get; set; }

        public string LinkID
        {
            get
            {
                if(Tag == null) return null;
                return Tag.Replace("#", "");
            }
        }

        public string LatestLinkID
        {
            get
            {
                if (LatestTag == null) return null;
                return LatestTag.Replace("#", "");
            }
        }
    }
}
