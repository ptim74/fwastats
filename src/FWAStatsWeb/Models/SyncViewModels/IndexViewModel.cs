using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.SyncViewModels
{
    public class IndexViewModel
    {
        public ICollection<WarSync> Syncs;

        public ICollection<SyncIndexClan> Clans { get; set; }
    }
}
