using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public enum SubmitState
    {
        Unknown,
        Queued,
        Running,
        Succeeded,
        Failed
    }

    public class SubmitResult
    {
        public DateTime Timestamp { get; set; }
        public SubmitState State { get; set; }
        public string Message { get; set; }
    }
}
