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
    public class WarOpponentBadgeUrls
    {
        public virtual WarOpponentResult WarOpponent { get; set; }

        [Key]
        [ForeignKey("WarOpponent")]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string WarID { get; set; }

        [StringLength(150)]
        [DataMember(Name = "small")]
        public string Small { get; set; }

        [StringLength(150)]
        [DataMember(Name = "medium")]
        public string Medium { get; set; }

        [StringLength(150)]
        [DataMember(Name = "large")]
        public string Large { get; set; }
    }
}
