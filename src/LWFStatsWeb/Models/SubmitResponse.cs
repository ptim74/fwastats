using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class SubmitResponse
    {
        public bool Status { get; set; }
        public string Details { get; set; }
        public SubmitError Error { get; set; }
    }
}
