using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public enum UpdateTaskMode
    {
        Update,
        Insert,
        Delete
    }

    public class UpdateTask
    {
        public Guid ID { get; set; }
        [StringLength(10)]
        public string ClanTag { get; set; }
        [StringLength(50)]
        public string ClanName { get; set; }
        [StringLength(10)]
        public string ClanGroup { get; set; }
        public UpdateTaskMode Mode { get; set; }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(ClanTag);
            }
        }
    }
}
