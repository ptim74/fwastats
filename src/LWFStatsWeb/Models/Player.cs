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
        //[StringLength(10)]
        //public string ClanTag { get; set; }

        [Key]
        [DataMember]
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Tag { get; set; }

        [DataMember]
        [StringLength(50)]
        public string Name { get; set; }

        [NotMapped]
        [DataMember]
        public int ExpLevel { get; set; }

        [NotMapped]
        [DataMember]
        public int Trophies { get; set; }

        [NotMapped]
        [DataMember]
        [StringLength(10)]
        public string Role { get; set; }

        [NotMapped]
        [DataMember]
        public int Donations { get; set; }

        [NotMapped]
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
        protected PlayerClan Clan { get; set; }

        [NotMapped]
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

        [NotMapped]
        [StringLength(30)]
        public string LeagueName { get; set; }

        [NotMapped]
        [StringLength(150)]
        public string BadgeUrl { get; set; }

        [NotMapped]
        [StringLength(10)]
        public string ClanTag { get; set; }

        [NotMapped]
        [StringLength(50)]
        public string ClanName { get; set; }

        public DateTime LastUpdated { get; set; }

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
            else
            {
                //TODO: Get image from league API and cache value
                //Hardcoded small image of Unranked league
                BadgeUrl = "https://api-assets.clashofclans.com/leagues/72/e--YMyIexEQQhE4imLoJcwhYn6Uy8KqlgyY3_kFV6t4.png";
            }
            if (Clan != null)
            {
                ClanTag = Clan.Tag;
                ClanName = Clan.Name;
            }
        }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }

        public string ClanLinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(ClanTag);
            }
        }

        public string RoleName
        {
            get
            {
                return Logic.Utils.FixRoleName(Role);
            }
        }
    }
}
