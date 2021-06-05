using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FWAStatsWeb.Models.DataViewModels
{
    [XmlRoot("Wars")]
    public class ClanWars : List<ClanWarModel>
    {
    }

    [XmlType("War")]
    public class ClanWarModel
    {
        public DateTime EndTime { get; set; }
        public string Result { get; set; }
        public int TeamSize { get; set; }

        public string ClanTag { get; set; }
        public string ClanName { get; set; }
        public int ClanLevel { get; set; }
        public int ClanStars { get; set; }
        public double ClanDestructionPercentage { get; set; }
        public int ClanAttacks { get; set; }
        public int ClanExpEarned { get; set; }
        //public string ClanBadgeUrl { get; set; }

        public string OpponentTag { get; set; }
        public string OpponentName { get; set; }
        public int OpponentLevel { get; set; }
        public int OpponentStars { get; set; }
        public double OpponentDestructionPercentage { get; set; }
        //public string OpponentBadgeUrl { get; set; }

        public bool Synced { get; set; }
        public bool Matched { get; set; }
    }
}
