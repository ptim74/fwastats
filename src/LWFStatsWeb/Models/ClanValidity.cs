using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class ClanValidity
    {
        [Key]
        [StringLength(10)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Tag { get; set; }

        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(10)]
        public string Group { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ValidFrom { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime ValidTo { get; set; }

        public bool IsValid()
        {
            var now = DateTime.UtcNow;
            if (ValidFrom <= now && ValidTo >= now)
                return true;
            return false;
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
