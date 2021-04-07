using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.PlayerViewModels
{
    public class SearchResultModel
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
        public DateTime LastSeen { get; set; }

        public string TimeDesc()
        {
            return Logic.Utils.TimeSpanToString(DateTime.UtcNow.Subtract(LastSeen));
        }
    }
}
