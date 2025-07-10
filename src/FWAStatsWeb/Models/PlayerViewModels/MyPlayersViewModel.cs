using System.Collections.Generic;

namespace FWAStatsWeb.Models.PlayerViewModels;

public class MyPlayersViewModel
{
    public ICollection<MyPlayerModel> Players { get; set; }
}
