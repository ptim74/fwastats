using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FWAStatsWeb.Models;

public class BlacklistedClan
{
    [Key]
    [StringLength(15)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Tag { get; set; }

    [StringLength(50)]
    public string Name { get; set; }
}
