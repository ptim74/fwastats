using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.UpdateViewModels
{
    public class PlayersViewModel
    {
        public List<string> Errors { get; set; }
        public List<PlayerUpdateTask> Tasks { get; set; }
    }
}
