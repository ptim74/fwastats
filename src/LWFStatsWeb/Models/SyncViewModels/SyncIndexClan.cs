using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.SyncViewModels
{
    public class SyncIndexClan
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string BadgeURL { get; set; }

        public string LinkID
        {
            get
            {
                return Tag.Replace("#", "");
            }
        }

        public List<SyncIndexResult> Results { get; set; }
    }
}
