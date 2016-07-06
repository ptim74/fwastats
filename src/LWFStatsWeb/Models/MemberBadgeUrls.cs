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
    public class MemberBadgeUrls
    {
        public virtual Member Member { get; set; }

        [Key]
        [ForeignKey("Member")]
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string MemberTag { get; set; }

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
