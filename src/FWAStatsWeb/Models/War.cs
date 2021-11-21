using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    [DataContract]
    public class War
    {
        [DataMember]
        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        [DataMember]
        [StringLength(15)]
        public string Result { get; set; }  // win

        [DataMember]
        public int TeamSize { get; set; } // 40

        [DataMember]
        private WarClan Clan { get; set; }

        [DataMember]
        private WarClan Opponent { get; set; }

        [DataMember]
        public DateTime PreparationStartTime { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public DateTime EndTime { get; set; }

        [StringLength(15)]
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

        [StringLength(15)]
        public string OpponentTag { get; set; }

        [StringLength(50)]
        public string OpponentName { get; set; }
        public int OpponentLevel { get; set; }
        public int OpponentStars { get; set; }
        public double OpponentDestructionPercentage { get; set; }
 
        [StringLength(150)]
        public string OpponentBadgeUrl { get; set; }

        public bool Synced { get; set; }
        public bool Matched { get; set; }
        public bool Friendly { get; set; }

        [NotMapped]
        public bool Blacklisted { get; set; }

        [NotMapped]
        [DataMember]
        public string State { get; set; }

        public virtual ICollection<WarAttack> Attacks { get; set; }

        public virtual ICollection<WarMember> Members { get; set; }

        public void FixData(DateTime previousEndTime)
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

                if (Clan.Members != null && Opponent.Members != null)
                {
                    FixMembers(Clan.Members, Opponent.Members, false);
                    FixMembers(Opponent.Members, Clan.Members, true);
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

            if (string.IsNullOrEmpty(Result) && !string.IsNullOrEmpty(State))
                Result = State;

            var timeSincePreviousWar = EndTime.Subtract(previousEndTime);

            if (ClanExpEarned == 0 && ClanStars > 0 && string.IsNullOrEmpty(State))
            {
                Friendly = true;
            }
            else if(timeSincePreviousWar.TotalHours <= 47.0 && timeSincePreviousWar.TotalMinutes > 1)
            {
                Friendly = true;
            }
            
        }

        private void FixMembers(ICollection <WarMember> members, ICollection<WarMember> opponentMembers, bool isOpponent)
        {
            if (Members == null)
                Members = new List<WarMember>();

            foreach (var member in members)
            {
                member.WarID = ID;
                member.IsOpponent = isOpponent;
                Members.Add(member);
                if (member.Attacks != null)
                {
                    foreach (var attack in member.Attacks)
                    {
                        attack.WarID = ID;
                        attack.IsOpponent = isOpponent;

                        var defender = opponentMembers.SingleOrDefault(m => m.Tag == attack.DefenderTag);
                        if (defender != null)
                        {
                            attack.DefenderMapPosition = defender.MapPosition;
                            attack.DefenderTownHallLevel = defender.TownHallLevel;
                        }

                        if (Attacks == null)
                            Attacks = new List<WarAttack>();

                        Attacks.Add(attack);
                    }
                }
            }
        }

        public string ClanLinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(ClanTag);
            }
        }

        public string OpponentLinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(OpponentTag);
            }
        }

        public DateTime SearchTime
        {
            get
            {
                if (PreparationStartTime != DateTime.MinValue)
                    return PreparationStartTime;
                if (EndTime == DateTime.MinValue)
                    return EndTime;
                return EndTime.AddHours(-47);
            }
        }

        public long WarLinkID
        {
            get
            {
                return Logic.Utils.WarTimeToId(EndTime);
            }
        }

    }
}
