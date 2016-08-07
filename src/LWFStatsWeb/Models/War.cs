using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    [DataContract]
    public class War
    {
        [DataMember]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        [DataMember]
        [StringLength(10)]
        public string Result { get; set; }  // win

        [DataMember]
        public int TeamSize { get; set; } // 40

        [DataMember]
        private WarClan Clan;

        [DataMember]
        private WarClan Opponent;

        [DataMember]
        public DateTime EndTime { get; set; }

        [StringLength(10)]
        public string ClanTag { get; set; }

        [StringLength(50)]
        public string ClanName { get; set; }
        public int ClanLevel { get; set; }
        public int ClanStars { get; set; }
        public double ClanDestructionPercentage { get; set; }
        public int ClanAttacks { get; set; }
        public int ClanExpEarned { get; set; }

        [StringLength(150)]
        public string ClanBadgeUrl { get; set; }

        [StringLength(10)]
        public string OpponentTag { get; set; }

        [StringLength(50)]
        public string OpponentName { get; set; }
        public int OpponentLevel { get; set; }
        public int OpponentStars { get; set; }
        public double OpponentDestructionPercentage { get; set; }
 
        [StringLength(150)]
        public string OpponentBadgeUrl { get; set; }

        public void FixData()
        {
            if (Clan != null)
            {
                ID = EndTime.ToUniversalTime().ToString("yyyyMMddTHHmmss") + Clan.Tag;
                ClanTag = Clan.Tag;
                ClanName = Clan.Name;
                ClanLevel = Clan.ClanLevel;
                ClanStars = Clan.Stars;
                ClanDestructionPercentage = Clan.DestructionPercentage;
                ClanAttacks = Clan.Attacks;
                ClanExpEarned = Clan.ExpEarned;

                if (Clan.BadgeUrls != null)
                {
                    ClanBadgeUrl = Clan.BadgeUrls.Small;
                }
            }

            if (Opponent != null)
            {
                OpponentTag = Opponent.Tag;
                OpponentName = Opponent.Name;
                OpponentLevel = Opponent.ClanLevel;
                OpponentStars = Opponent.Stars;
                OpponentDestructionPercentage = Opponent.DestructionPercentage;

                if (Opponent.BadgeUrls != null)
                {
                    OpponentBadgeUrl = Opponent.BadgeUrls.Small;
                }
            }
        }

        public string ClanLinkID
        {
            get
            {
                return ClanTag.Replace("#", "");
            }
        }

        public string OpponentLinkID
        {
            get
            {
                return OpponentTag.Replace("#", "");
            }
        }

    }
}
