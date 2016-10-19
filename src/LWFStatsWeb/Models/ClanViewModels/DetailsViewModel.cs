using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class DetailsViewModel
    {
        public bool InAlliance { get; set; }
        public ClanValidity Validity { get; set; }
        public Clan Clan { get; set; }
        public List<ClanDetailsEvent> Events { get; set; }
    }
}
