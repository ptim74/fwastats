using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.PlayerViewModels
{
    public class PlayerDetailsEvent
    {
        public string Tag { get; set; }

        public string Name { get; set; }

        public PlayerEventType EventType { get; set; }

        public string Value { get; set; }

        public DateTime EventDate { get; set; }

        public string TimeDesc { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
