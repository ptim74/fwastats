namespace FWAStatsWeb.Models.UpdateViewModels;

public class IndexViewModel
{
    public ICollection<string> Errors { get; set; }
    public ICollection<UpdateTask> Tasks { get; set; }
}
