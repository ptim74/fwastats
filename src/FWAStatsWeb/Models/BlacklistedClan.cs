using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class BlacklistedClan
    {
        [Key]
        [StringLength(15)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Tag { get; set; }

        [StringLength(50)]
        public string Name { get; set; }
    }
}
