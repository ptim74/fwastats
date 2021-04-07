using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.ClanViewModels
{
    public class DonationTrackModel
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Donated { get; set; }
        public int Received { get; set; }
    }
}
