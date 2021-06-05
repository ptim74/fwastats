using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public class SubmitEntry
    {
        public SubmitRequest Request { get; set; }
        //public SubmitResponse Response { get; set; }
        public SubmitStatus Status { get; set; }
    }
}
