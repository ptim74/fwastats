using System.ComponentModel.DataAnnotations;

namespace FWAStatsWeb.Models.PlayerViewModels;

public class UnlinkViewModel
{
    [Required]
    [Display(Name = "Player Tag")]
    public string Tag { get; set; }
}
