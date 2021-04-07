using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public class Donation
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Donated { get; set; }
        public int Received { get; set; }
    }
}
