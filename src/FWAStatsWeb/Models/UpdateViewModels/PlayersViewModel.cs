using System.Collections.Generic;

namespace FWAStatsWeb.Models.UpdateViewModels;

public class PlayersViewModel
{
    public ICollection<string> Errors { get; set; }
    public ICollection<PlayerUpdateTask> Tasks { get; set; }
}
