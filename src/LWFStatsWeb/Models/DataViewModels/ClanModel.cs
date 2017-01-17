using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LWFStatsWeb.Models.DataViewModels
{
    [XmlRoot("Clans")]
    public class Clans : List<ClanModel>
    {
    }

    [XmlType("Clan")]
    public class ClanModel
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int Points { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public int RequiredTrophies { get; set; }
        public string WarFrequency { get; set; }
        public int WinStreak { get; set; }
        public int Wins { get; set; }
        public int Ties { get; set; }
        public int Losses { get; set; }
        public bool IsWarLogPublic { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
    }
}
