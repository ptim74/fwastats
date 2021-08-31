using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.PlayerViewModels
{
    public class UnlinkViewModel
    {
        [Required]
        [Display(Name = "Player Tag")]
        public string Tag { get; set; }
    }
}
