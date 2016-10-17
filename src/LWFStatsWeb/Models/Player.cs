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
    public class Player
    {
        //[ForeignKey("Clan")]
        //[DataMember]
        [StringLength(10)]
        public string ClanTag { get; set; }

        [Key]
        [DataMember]
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Tag { get; set; }

        [DataMember]
        [StringLength(50)]
        public string Name { get; set; }

        [DataMember]
        public int ExpLevel { get; set; }

        [DataMember]
        public int Trophies { get; set; }

        [DataMember]
        [StringLength(10)]
        public string Role { get; set; }

        //[DataMember]
        //public int ClanRank { get; set; }

        [DataMember]
        public int Donations { get; set; }

        [DataMember]
        public int DonationsReceived { get; set; }

        [DataMember]
        public int AttackWins { get; set; }

        [DataMember]
        public int DefenseWins { get; set; }

        [DataMember]
        public int BestTrophies { get; set; }

        [DataMember]
        public int WarStars { get; set; }

        [DataMember]
        public int TownHallLevel { get; set; }

        [NotMapped]
        [DataMember]
        public PlayerClan Clan { get; set; }

        [DataMember]
        private League League { get; set; }

        [NotMapped]
        [DataMember]
        public virtual ICollection<PlayerAchievement> Achievements { get; set; }

        [NotMapped]
        [DataMember]
        public virtual ICollection<PlayerResource> Troops { get; set; }

        [NotMapped]
        [DataMember]
        public virtual ICollection<PlayerResource> Heroes { get; set; }

        [NotMapped]
        [DataMember]
        public virtual ICollection<PlayerResource> Spells { get; set; }

        [StringLength(30)]
        public string LeagueName { get; set; }

        [StringLength(150)]
        public string BadgeUrl { get; set; }

        public void FixData()
        {
            if (League != null)
            {
                LeagueName = League.Name;

                if (League.IconUrls != null)
                {
                    BadgeUrl = League.IconUrls.Small;
                }
            }
        }

        public string LinkID
        {
            get
            {
                return Tag.Replace("#", "");
            }
        }
    }
}
