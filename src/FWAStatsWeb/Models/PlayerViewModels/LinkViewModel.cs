using System.ComponentModel.DataAnnotations;

namespace FWAStatsWeb.Models.PlayerViewModels;

public class LinkViewModel
{
    [Required]
    [Display(Name = "Player Tag")]
    public string Tag { get; set; }

    [Required]
    [Display(Name = "API Token")]
    public string ApiToken { get; set; }
}
