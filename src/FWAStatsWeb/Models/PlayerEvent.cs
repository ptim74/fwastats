using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public enum PlayerEventType
    {
        Join = 1,
        Leave = 2,
        Stars = 3,
        Townhall = 4,
        Promote = 5,
        Demote = 6,
        NameChange = 7
    }

    public class PlayerEvent
    {
        public int ID { get; set; }

        [StringLength(15)]
        public string ClanTag { get; set; }

        [StringLength(15)]
        public string PlayerTag { get; set; }

        public PlayerEventType EventType { get; set; }

        public int Value { get; set; }

        [StringLength(15)]
        public string StringValue { get; set; }

        public DateTime EventDate { get; set; }

        [NotMapped]
        public string Role {
            get
            {
                return ValueToRole(Value);
            }
            set
            {
                Value = RoleToValue(value);
            }
        }

        [NotMapped]
        public string RoleName
        {
            get
            {
                switch (Value)
                {
                    case 3:
                        return "Leader";
                    case 2:
                        return "Co-leader";
                    case 1:
                        return "Elder";
                    default:
                        return "Member";
                }
            }
        }

        public static int RoleToValue(string role)
        {
            switch (role)
            {
                case "leader":
                    return 3;
                case "coLeader":
                    return 2;
                case "admin":
                    return 1;
                default:
                    return 0;
            }
        }

        public static string ValueToRole(int value)
        {
            switch (value)
            {
                case 3:
                    return "leader";
                case 2:
                    return "coLeader";
                case 1:
                    return "admin";
                default:
                    return "member";
            }
        }

        public string TimeDesc()
        {
            return Logic.Utils.TimeSpanToString(DateTime.UtcNow.Subtract(EventDate));
        }

    }
}
