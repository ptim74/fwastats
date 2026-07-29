namespace FWAStatsWeb.Models.SyncViewModels;

public class IndexViewModel
{
    public ICollection<WarSync> Syncs;

    public ICollection<SyncIndexClan> Clans { get; set; }
}
