using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class IndexViewModel : List<ClanIndexClan>
    {
        public string Group { get; set; }
    }
}
