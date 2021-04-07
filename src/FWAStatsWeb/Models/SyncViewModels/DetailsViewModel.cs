using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.SyncViewModels
{
    public class DetailsViewModel
    {
        public int TeamSize { get; set; }
        public WarSync Sync { get; set; }
        public ICollection<War> Wars { get; set; }
    }
}
