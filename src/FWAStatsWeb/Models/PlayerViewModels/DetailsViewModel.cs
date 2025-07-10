using System.Collections.Generic;

namespace FWAStatsWeb.Models.PlayerViewModels;

public class DetailsViewModel
{
    public Player Player { get; set; }
    public ICollection<PlayerDetailsEvent> Events { get; set; } 
    public bool Claimed { get; set; }
}
