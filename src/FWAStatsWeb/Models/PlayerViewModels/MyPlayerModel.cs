using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.PlayerViewModels
{
    public class MyPlayerModel
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
        public string ClanTag { get; set; }
        public string ClanName { get; set; }
        public string ClanLinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(ClanTag);
            }
        }

        public bool IsFWA { get; set; }

        public SubmitRestriction ClanSubmitRestriction { get; set; }
    }
}
