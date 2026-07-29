namespace FWAStatsWeb.Models.PlayerViewModels;

public class SearchViewModel
{
    public string Query { get; set; }
    public ICollection<SearchResultModel> Results { get; set; }
}
