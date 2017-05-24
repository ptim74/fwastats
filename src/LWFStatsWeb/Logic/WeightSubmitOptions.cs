using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class WeightSubmitOptions
    {
        public string ClientEmail { get; set; }
        public string PrivateKey { get; set; }
        public string SheetId { get; set; }
        public string TabName { get; set; }
        public string ClanNameRange { get; set; }
        public string CompositionRange { get; set; }
        public string WeightRange { get; set; }
        public string CheckRange { get; set; }
        public string TagRange { get; set; }
        public string THRange { get; set; }
        public string AutoSubmitRange { get; set; }
        public string ResultRange { get; set; }
    }
}
