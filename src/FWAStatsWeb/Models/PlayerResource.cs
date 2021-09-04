using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public class PlayerResource
    {
        public string Name { get; set; }
        public int Level { get; set; }
        public int MaxLevel { get; set; }

        public string Village { get; set; }
    }
}
