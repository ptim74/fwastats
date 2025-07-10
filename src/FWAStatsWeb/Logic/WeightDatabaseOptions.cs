namespace FWAStatsWeb.Logic;

public class WeightDatabaseOptions
{
    public string Url { get; set; }
    public string ResourceName { get; set; }
    public int SinceHours { get; set; }
    public string SheetId { get; set; }
    public string Range { get; set; }
    public int TagColumn { get; set; }
    public int WeightColumn { get; set; }
    public int TimestampColumn { get; set; }
}
