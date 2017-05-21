using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class WeightSubmitModel
    {
        public string ClanName { get; set; }
        public string ClanTag { get; set; }
        public string ClanLink { get; set; }
        public string ClanBadge { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
        public string SheetUrl { get; set; }
    }
}
