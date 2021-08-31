using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.PlayerViewModels
{
    public class LinkViewModel
    {
        [Required]
        [Display(Name = "Player Tag")]
        public string Tag { get; set; }

        [Required]
        [Display(Name = "API Token")]
        public string ApiToken { get; set; }
    }
}
