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

        public string OpponentTag { get; set; }
        public string OpponentName { get; set; }
        public int Attacks { get; set; }

        public string Defender1Tag { get; set; }
        public string Defender1Name { get; set; }
        public int Defender1TownHall { get; set; }
        public int Defender1Position { get; set; }
        public int Stars1 { get; set; }
        public int DesctuctionPercentage1 { get; set; }

        public string Defender2Tag { get; set; }
        public string Defender2Name { get; set; }
        public int Defender2TownHall { get; set; }
        public int Defender2Position { get; set; }
        public int Stars2 { get; set; }
        public int DesctuctionPercentage2 { get; set; }
    }
}
