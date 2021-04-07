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
    public class WarMember
    {
        public int ID { get; set; }

        [ForeignKey("War")]
        [StringLength(30)]
        public string WarID { get; set; }

        [DataMember]
        [StringLength(15)]
        public string Tag { get; set; }

        [DataMember]
        [StringLength(50)]
        public string Name { get; set; }

        [DataMember]
        public int TownHallLevel { get; set; }

        [DataMember]
        public int MapPosition { get; set; }

        [DataMember]
        public int OpponentAttacks { get; set; }

        [NotMapped]
        [DataMember]
        public ICollection<WarAttack> Attacks { get; set; }

        [NotMapped]
        [DataMember]
        public WarAttack BestOpponentAttack { get; set; }

        public bool IsOpponent { get; set; }
    }
}
