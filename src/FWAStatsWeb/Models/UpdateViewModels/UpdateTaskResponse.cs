using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.UpdateViewModels
{
    public class UpdateTaskResponse
    {
        public string ID { get; set; }
        public bool Status { get; set; }
        public string Message { get; set; }
    }
}
