using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWFStatsJob
{
    public class UpdateTask
    {
        public Guid ID { get; set; }
        public string ClanTag { get; set; }
        public string ClanName { get; set; }
        public TaskUpdateMode Mode { get; set; }
    }
}
