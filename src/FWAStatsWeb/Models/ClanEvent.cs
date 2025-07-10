using System;
using System.ComponentModel.DataAnnotations;

namespace FWAStatsWeb.Models;

public class ClanEvent
{
    public int ID { get; set; }

    [StringLength(15)]
    public string ClanTag { get; set; }

    public int Donations { get; set; }

    public int Activity { get; set; }

    public DateTime EventDate { get; set; }
}
