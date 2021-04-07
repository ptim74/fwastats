using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FWAStatsWeb.Models.DataViewModels
{
    [XmlRoot("Members")]
    public class ClanMembers : List<ClanMemberModel>
    {
    }

    [XmlType("Member")]
    public class ClanMemberModel
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public int Level { get; set; }
        public int Donated { get; set; }
        public int Received { get; set; }
        public int Rank { get; set; }
        public int Trophies { get; set; }
        public string League { get; set; }
        public int TownHall { get; set; }
        public int Weight { get; set; }
        public bool InWar { get; set; }
    }
}
