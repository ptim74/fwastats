using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class FormerClan
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string BadgeURL { get; set; }
        public string Group { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }

        public string LinkID
        {
            get
            {
                if(Tag == null) return null;
                return Tag.Replace("#", "");
            }
        }

    }
}
