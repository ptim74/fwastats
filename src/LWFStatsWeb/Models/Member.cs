using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    [DataContract]
    public class Member
    {
        [ForeignKey("Clan")]
        [StringLength(10)]
        public string ClanTag { get; set; }

        [Key]
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember(Name = "tag")]
        public string Tag { get; set; }

        [StringLength(50)]
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "expLevel")]
        public int ExpLevel { get; set; }

        [DataMember(Name = "trophies")]
        public int Trophies { get; set; }

        [StringLength(10)]
        [DataMember(Name = "role")]
        public string Role { get; set; }

        [DataMember(Name = "clanRank")]
        public int ClanRank { get; set; }

        [DataMember(Name = "donations")]
        public int Donations { get; set; }

        [DataMember(Name = "donationsReceived")]
        public int DonationsReceived { get; set; }

        public virtual Clan Clan { get; set; }

        [DataMember(Name = "badgeUrls")]
        public virtual MemberBadgeUrls BadgeUrl { get; set; }
    }
}
