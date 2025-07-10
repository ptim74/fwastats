namespace FWAStatsWeb.Models.UpdateViewModels;

public class PlayerUpdateTask
{
    public string Tag { get; set; }
    public string Name { get; set; }

    public string LinkID
    {
        get
        {
            return Logic.Utils.TagToLinkId(Tag);
        }
    }
}
