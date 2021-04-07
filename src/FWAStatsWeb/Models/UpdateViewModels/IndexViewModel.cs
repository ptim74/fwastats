using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.UpdateViewModels
{
    public class IndexViewModel
    {
        public ICollection<string> Errors { get; set; }
        public ICollection<UpdateTask> Tasks { get; set; }
    }
}
