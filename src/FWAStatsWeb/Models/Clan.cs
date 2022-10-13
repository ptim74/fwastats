using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    [DataContract]
    public class Clan 
    {
        [Key]
        [DataMember]
        [StringLength(15)]
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
        [StringLength(15)]
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

        [DataMember]
        public SubmitRestriction SubmitRestriction { get; set; }

        public int WarCount { get; set; }
        public double MatchPercentage { get; set; }
        public double WinPercentage { get; set; }

        public int Th15Count { get; set; }
        public int Th14Count { get; set; }
        public int Th13Count { get; set; }
        public int Th12Count { get; set; }
        public int Th11Count { get; set; }
        public int Th10Count { get; set; }
        public int Th9Count { get; set; }
        public int Th8Count { get; set; }
        public int ThLowCount { get; set; }
        public int EstimatedWeight { get; set; }

        public bool InLeague { get; set; }

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

            this.FixWars();
        }

        public void FixWars()
        {
            if (Wars != null)
            {
                var previousEndDate = DateTime.UtcNow.AddYears(-2);
                foreach (var war in Wars.OrderBy(w => w.EndTime))
                {
                    war.FixData(previousEndDate);
                    previousEndDate = war.EndTime;
                }
            }
        }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
