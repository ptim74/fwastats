using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.SyncViewModels
{
    public class DetailsViewModel
    {
        public WarSync Sync { get; set; }
        public ICollection<War> Wars { get; set; }
    }
}
