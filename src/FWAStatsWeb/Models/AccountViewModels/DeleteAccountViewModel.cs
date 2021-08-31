using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.AccountViewModels
{
    public class DeleteAccountViewModel
    {
        [Display(Name = "I want to delete my account")]
        public bool DeleteAccount { get; set; }

        [Display(Name = "Yes, I really want to delete my account")]
        public bool DeleteAccountConfirmation { get; set; }
    }
}
