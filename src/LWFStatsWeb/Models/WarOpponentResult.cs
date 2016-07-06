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
    public class WarOpponentResult
    {
        [Key]
        [ForeignKey("War")]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string WarID { get; set; }

        public virtual War War { get; set; }

        [StringLength(10)]
        [DataMember(Name = "tag")]
        public string Tag { get; set; }

        [StringLength(50)]
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "clanLevel")]
        public int ClanLevel { get; set; }

        [DataMember(Name = "stars")]
        public int Stars { get; set; }

        [DataMember(Name = "destructionPercentage")]
        public double DestructionPercentage { get; set; }

        [DataMember(Name = "badgeUrls")]
        public virtual WarOpponentBadgeUrls BadgeUrl { get; set; }
    }
}
