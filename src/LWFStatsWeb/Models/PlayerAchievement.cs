using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class PlayerAchievement
    {
        public string Name { get; set; }

        public int Stars { get; set; }

        public int Value { get; set; }

        public int Target { get; set; }

        public string CompletionInfo { get; set; }

        public string Info { get; set; }
    }
}
