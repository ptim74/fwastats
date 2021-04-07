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
        public string BadgeUrl { get; set; }
        public bool New { get; set; }
        public bool Departed { get; set; }
        public bool HiddenLog { get; set; }
        public bool InLeague { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }

        public ICollection<SyncIndexResult> Results { get; set; }
    }
}
