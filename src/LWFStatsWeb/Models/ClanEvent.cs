using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class ClanEvent
    {
        public int ID { get; set; }

        [StringLength(10)]
        public string ClanTag { get; set; }

        public int Donations { get; set; }

        public int Activity { get; set; }

        public DateTime EventDate { get; set; }
    }
}
