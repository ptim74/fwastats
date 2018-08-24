using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class WarAttack
    {
        public int ID { get; set; }

        [ForeignKey("War")]
        [StringLength(30)]
        public string WarID { get; set; }

        [DataMember]
        [StringLength(15)]
        public string AttackerTag { get; set; }

        [DataMember]
        [StringLength(15)]
        public string DefenderTag { get; set; }

        public int DefenderTownHallLevel { get; set; }

        public int DefenderMapPosition { get; set; }

        [DataMember]
        public int Stars { get; set; }

        [DataMember]
        public int DestructionPercentage { get; set; }

        [DataMember]
        public int Order { get; set; }

        public bool IsOpponent { get; set; }
    }
}
