using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.ClanViewModels
{
    public class SubmitAccessViewModel
    {
        public string ClanName { get; set; }
        public string ClanTag { get; set; }
        public SubmitRestriction SubmitRestriction { get; set; }
    }
}
