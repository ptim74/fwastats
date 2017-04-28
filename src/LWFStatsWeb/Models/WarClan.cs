using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    [DataContract]
    public class WarClan
    {
        [DataMember]
        public string Tag { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int ClanLevel { get; set; }

        [DataMember]
        public int Stars { get; set; }

        [DataMember]
        public double DestructionPercentage { get; set; }

        [DataMember]
        public int Attacks { get; set; }

        [DataMember]
        public int ExpEarned { get; set; }

        [DataMember]
        public virtual BadgeUrls BadgeUrls { get; set; }

        [DataMember]
        public virtual ICollection<WarMember> Members { get; set; }
    }
}
