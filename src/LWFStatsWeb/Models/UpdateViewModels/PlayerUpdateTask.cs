using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.UpdateViewModels
{
    public class PlayerUpdateTask
    {
        public string Tag { get; set; }
        public string Name { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
