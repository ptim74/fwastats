using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.SyncViewModels
{
    public class DetailsViewModel
    {
        public string Filter { get; set; }
        public WarSync Sync { get; set; }
        public List<War> Wars { get; set; }
    }
}
