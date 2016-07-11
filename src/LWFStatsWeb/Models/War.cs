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
        [ForeignKey("Clan")]
        [StringLength(10)]
        public string ClanTag { get; set; }

        [StringLength(30)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string ID { get; set; }

        [DataMember(Name = "result")]
        [StringLength(10)]
        public string Result { get; set; }  // win

        [DataMember(Name = "endTime")]
        private string endTime { get; set; }

        [DataMember(Name = "teamSize")]
        public int TeamSize { get; set; } // 40

        [DataMember(Name = "clan")]
        public virtual WarClanResult ClanResult { get; set; }

        [DataMember(Name = "opponent")]
        public virtual WarOpponentResult OpponentResult { get; set; }

        public DateTime EndTime { get; set; }

        public virtual Clan Clan { get; set; }

        public void FixData(string clanTag)
        {
            ClanTag = clanTag;
            ID = endTime + ClanTag;
            EndTime = DateTime.ParseExact(endTime, "yyyyMMddTHHmmss.fffK", CultureInfo.InvariantCulture);
            if (this.ClanResult != null)
            {
                this.ClanResult.WarID = ID;
            }
            if (this.OpponentResult != null)
            {
                this.OpponentResult.WarID = ID;
                if (this.OpponentResult.BadgeUrl != null)
                {
                    this.OpponentResult.BadgeUrl.WarID = ID;
                }
            }
        }
    }
}
