using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.ClanViewModels
{
    public class ClanAttackModel
    {
        public string Tag { get; set; }

        public string Name { get; set; }

        public string BadgeUrl { get; set; }

        public ICollection<ClanAttackMember> Members { get; set; }

        public ICollection<ClanAttackWar> Wars { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }

    public class ClanAttackWar
    {
        public string ID { get; set; }
        public string OpponentName { get; set; }
    }

    public class ClanAttackMember
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public IDictionary<string,ICollection<WarAttack>> Attacks { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }

}
