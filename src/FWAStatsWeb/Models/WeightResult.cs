using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FWAStatsWeb.Models;

public class WeightResult
{
    [Key]
    [StringLength(15)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Tag { get; set; }
    public DateTime Timestamp { get; set; }
    public int TeamSize { get; set; }
}
