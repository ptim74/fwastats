using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.UpdateViewModels
{
    public class PlayersViewModel
    {
        public ICollection<string> Errors { get; set; }
        public ICollection<PlayerUpdateTask> Tasks { get; set; }
    }
}
