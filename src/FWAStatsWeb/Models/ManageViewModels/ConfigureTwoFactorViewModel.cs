using Microsoft.AspNetCore.Mvc.Rendering;

namespace FWAStatsWeb.Models.ManageViewModels;

public class ConfigureTwoFactorViewModel
{
    public string SelectedProvider { get; set; }

    public ICollection<SelectListItem> Providers { get; set; }
}
