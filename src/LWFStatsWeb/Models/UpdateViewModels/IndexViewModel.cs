using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.UpdateViewModels
{
    public class IndexViewModel
    {
        public List<string> Errors { get; set; }
        public List<UpdateTask> Tasks { get; set; }
    }
}
