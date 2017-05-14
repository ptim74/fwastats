using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LWFStatsWeb.Models.DataViewModels
{
    [XmlRoot("WarMembers")]
    public class WarMembers : List<WarMemberModel>
    {
    }

    [XmlType("WarMember")]
    public class WarMemberModel
    {
        public int Position { get; set; }
        public string Tag { get; set; }
        public string Name { get; set; }
        public int TownHall { get; set; }
        public int Weight { get; set; }
    }
}
