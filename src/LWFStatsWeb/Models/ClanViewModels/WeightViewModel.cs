using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class WeightViewModel
    {
        public long WarID { get; set; }
        public string ClanLink { get; set; }
        public string ClanTag { get; set; }
        public string ClanName { get; set; }
        public string ClanBadge { get; set; }

        public virtual ICollection<MemberWeightModel> Members { get; set; }

        public virtual ICollection<WeightWarModel> Wars { get; set; }
    }

    public class WeightWarModel
    {
        public long ID { get; set; }
        public string OpponentName { get; set; }
    }
}
