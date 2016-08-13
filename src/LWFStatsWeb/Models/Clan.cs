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
    public class Clan 
    {
        [Key]
        [DataMember]
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Tag { get; set; }

        [DataMember]
        [StringLength(50)]
        public string Name { get; set; }

        [DataMember]
        public int ClanLevel { get; set; }

        [DataMember]
        public int ClanPoints { get; set; }

        [DataMember]
        [StringLength(10)]
        public string Type { get; set; }

        [DataMember]
        [StringLength(10)]
        public string Group { get; set; }

        [DataMember]
        public int Members { get; set; }

        [DataMember]
        public int RequiredTrophies { get; set; }

        [DataMember]
        [StringLength(20)]
        public string WarFrequency { get; set; }

        [DataMember]
        public int WarWinStreak { get; set; }

        [DataMember]
        public int WarWins { get; set; }

        [DataMember]
        public int WarTies { get; set; }

        [DataMember]
        public int WarLosses { get; set; }

        [DataMember]
        public bool IsWarLogPublic { get; set; }

        [DataMember]
        [StringLength(300)]
        public string Description { get; set; }

        [DataMember]
        public virtual ICollection<Member> MemberList { get; set; }

        [DataMember]
        private Location Location { get; set; }

        [StringLength(30)]
        public string LocationName { get; set; }

        [NotMapped]
        [DataMember]
        public virtual ICollection<War> Wars { get; set; }

        [DataMember]
        private BadgeUrls BadgeUrls { get; set; }

        [DataMember]
        [StringLength(150)]
        public string BadgeUrl { get; set; }

        public void FixData()
        {
            if(BadgeUrls != null)
            {
                BadgeUrl = BadgeUrls.Small;
            }

            if(Location != null)
            {
                LocationName = Location.Name;
            }

            if (MemberList != null)
            {
                foreach (var member in MemberList)
                {
                    member.FixData(Tag);
                }
            }

            if (Wars != null)
            {
                foreach (var war in Wars)
                {
                    war.FixData();
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
