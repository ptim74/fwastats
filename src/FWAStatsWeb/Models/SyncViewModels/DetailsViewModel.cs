using System.Collections.Generic;

namespace FWAStatsWeb.Models.SyncViewModels;

public class DetailsViewModel
{
    public int TeamSize { get; set; }
    public WarSync Sync { get; set; }
    public ICollection<War> Wars { get; set; }
}
