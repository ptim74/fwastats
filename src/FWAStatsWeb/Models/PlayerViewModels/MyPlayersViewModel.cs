using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.PlayerViewModels
{
    public class MyPlayersViewModel
    {
        public ICollection<MyPlayerModel> Players { get; set; }
    }
}
