using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public class PlayerClan
    {
        public string Tag { get; set; }

        public string Name { get; set; }

        public int ClanLevel { get; set; }

        protected BadgeUrls BadgeUrls { get; set; }

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
