using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWFStatsJob
{
    public class UpdateIndexView
    {
        public List<string> Errors { get; set; }
        public List<UpdateTask> Tasks { get; set; }
    }
}
