using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.PlayerViewModels
{
    public class DetailsViewModel
    {
        public Player Player { get; set; }
        public ICollection<PlayerDetailsEvent> Events { get; set; } 
        public bool Claimed { get; set; }
    }
}
