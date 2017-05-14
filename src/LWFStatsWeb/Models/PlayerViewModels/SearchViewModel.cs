using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.PlayerViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public ICollection<SearchResultModel> Results { get; set; }
    }
}
