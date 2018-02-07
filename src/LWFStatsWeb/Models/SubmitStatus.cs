using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class SubmitStatus
    {
        public SubmitPhase Phase { get; set; }
        public DateTime Timestamp { get; set; }
        public String Message { get; set; }

        public void UpdatePhase(SubmitPhase phase)
        {
            this.Phase = phase;
            this.Timestamp = DateTime.UtcNow;
        }
    }
}
