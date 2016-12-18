using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.PlayerViewModels
{
    public class DetailsViewModel
    {
        public Player Player { get; set; }
        public List<PlayerDetailsEvent> Events { get; set; } 
    }
}
