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
                return Value switch
                {
                    3 => "Leader",
                    2 => "Co-leader",
                    1 => "Elder",
                    _ => "Member",
                };
            }
        }

        public static int RoleToValue(string role)
        {
            return role switch
            {
                "leader" => 3,
                "coLeader" => 2,
                "admin" => 1,
                _ => 0,
            };
        }

        public static string ValueToRole(int value)
        {
            return value switch
            {
                3 => "leader",
                2 => "coLeader",
                1 => "admin",
                _ => "member",
            };
        }

        public string TimeDesc()
        {
            return Logic.Utils.TimeSpanToString(DateTime.UtcNow.Subtract(EventDate));
        }

    }
}
