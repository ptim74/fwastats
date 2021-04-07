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
        public DateTime LatestDate { get; set; }
        public bool Blacklisted { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }

        public string LatestLinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(LatestTag);
            }
        }
    }
}
