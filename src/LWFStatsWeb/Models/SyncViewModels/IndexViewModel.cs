using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.SyncViewModels
{
    public class IndexViewModel
    {
        public List<WarSync> Syncs;

        public List<SyncIndexClan> Clans { get; set; }
    }
}
