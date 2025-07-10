using System.ComponentModel.DataAnnotations;

namespace FWAStatsWeb.Models.AccountViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
