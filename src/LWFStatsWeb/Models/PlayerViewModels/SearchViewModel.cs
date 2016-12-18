using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.PlayerViewModels
{
    public class SearchViewModel
    {
        public string Query { get; set; }
        public List<SearchResultModel> Results { get; set; }
    }
}
