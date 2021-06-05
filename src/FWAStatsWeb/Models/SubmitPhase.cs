using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public enum SubmitPhase
    {
        Unknown,
        Queued,
        Running,
        Succeeded,
        Failed
    }
}
