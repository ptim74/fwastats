using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.ClanViewModels
{
    public class IndexViewModel : List<ClanIndexClan>
    {
        public bool IsMyClans { get; set; }
    }
}
