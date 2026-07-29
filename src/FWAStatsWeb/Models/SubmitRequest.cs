namespace FWAStatsWeb.Models;

public class SubmitRequest
{
    public string ClanTag { get; set; }
    public string ClanName { get; set; }
    public string Mode { get; set; }
    public ICollection<SubmitMember> Members { get; set; }
}
