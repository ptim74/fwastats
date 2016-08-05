using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.SyncViewModels
{
    public class SyncIndexResult
    {
        public string Result { get; set; }
        public bool IsAlliance { get; set; }
        public string OpponentName { get; set; }
        public string OpponentTag { get; set; }
        public string OpponentBadgeURL { get; set; }

        public string OpponentLinkID
        {
            get
            {
                if (OpponentTag == null)
                    return null;
                return OpponentTag.Replace("#", "");
            }
        }
    }
}
