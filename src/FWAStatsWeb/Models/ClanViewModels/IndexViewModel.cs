using System.Collections.Generic;

namespace FWAStatsWeb.Models.ClanViewModels;

public class IndexViewModel : List<ClanIndexClan>
{
    public bool IsMyClans { get; set; }
}
