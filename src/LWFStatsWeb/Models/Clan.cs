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
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DataMember(Name = "tag")]
        public string Tag { get; set; }

        [StringLength(50)]
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "clanLevel")]
        public int ClanLevel { get; set; }

        [DataMember(Name = "clanPoints")]
        public int ClanPoints { get; set; }

        [StringLength(10)]
        [DataMember(Name = "type")]
        public string ClanType { get; set; }

        [DataMember(Name = "members")]
        public int MemberCount { get; set; }

        [DataMember(Name = "requiredTrophies")]
        public int RequiredTrophies { get; set; }

        [DataMember(Name = "warWinStreak")]
        public int WarWinStreak { get; set; }

        [DataMember(Name = "warWins")]
        public int WarWins { get; set; }

        [DataMember(Name = "warTies")]
        public int WarTies { get; set; }

        [DataMember(Name = "warLosses")]
        public int WarLosses { get; set; }

        [DataMember(Name = "isWarLogPublic")]
        public bool IsWarLogPublic { get; set; }

        [StringLength(300)]
        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "memberList")]
        public virtual ICollection<Member> Members { get; set; }

        public virtual ICollection<War> Wars { get; set; }

        [DataMember(Name = "badgeUrls")]
        public virtual ClanBadgeUrls BadgeUrl { get; set; }

        public void FixData()
        {
            if (Members != null)
            {
                foreach (var member in Members)
                {
                    member.ClanTag = Tag;
                    if (member.BadgeUrl != null)
                        member.BadgeUrl.MemberTag = member.Tag;
                }
            }

            if (BadgeUrl != null)
            {
                BadgeUrl.ClanTag = Tag;
            }

            if (Wars != null)
            {
                foreach (var war in Wars)
                {
                    war.ClanTag = Tag;
                    war.FixData();
                }
            }
        }
    }
}
