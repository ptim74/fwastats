using System.ComponentModel.DataAnnotations;

namespace FWAStatsWeb.Models.AccountViewModels;

public class DeleteAccountViewModel
{
    [Display(Name = "I want to delete my account")]
    public bool DeleteAccount { get; set; }

    [Display(Name = "Yes, I really want to delete my account")]
    public bool DeleteAccountConfirmation { get; set; }
}
