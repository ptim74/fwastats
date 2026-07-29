namespace FWAStatsWeb.Models.HomeViewModels;

public class IndexViewModel
{
    public IDictionary<int, SyncStats> LastStats { get; set; }
    public ICollection<SyncStats> SyncHistory { get; set; }
    public CounterStats Counters { get; set; }
}

public class SyncStats
{
    public int ID { get; set; }
    public string DisplayName { get; set; }
    public string Status { get; set; }
    public int AllianceMatches { get; set; }
    public int WarMatches { get; set; }
    public int NotStarted { get; set; }
}

//public class SyncHistory
//{
//    public ICollection<SyncStats> Syncs { get; set; }
//}

public class CounterStats
{
    public int ClanCount { get; set; }
    public int MemberCount { get; set; }
    public double MatchPercentage { get; set; }
    public double WinPercentage { get; set; }
    public int TeamSize45Wars { get; set; }
    public int TeamSize50Wars { get; set; }
    public int ClansInLeague { get; set; }
}

public class ClanDetails
{
    public string Tag { get; set; }
    public string Name { get; set; }
    public int Members { get; set; }
    public string BadgeUrl { get; set; }

    public string LinkID
    {
        get
        {
            return Logic.Utils.TagToLinkId(Tag);
        }
    }
}
