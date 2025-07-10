using System.ComponentModel.DataAnnotations;

namespace FWAStatsWeb.Models.ManageViewModels;

public class VerifyEmailViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
