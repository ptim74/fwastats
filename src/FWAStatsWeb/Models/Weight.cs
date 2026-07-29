using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FWAStatsWeb.Models;

public class Weight
{
    [Key]
    [StringLength(15)]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Tag { get; set; }
    public int WarWeight { get; set; }
    public int ExtWeight { get; set; }
    public int SyncWeight { get; set; }
    public bool InWar { get; set; }
    public DateTime LastModified { get; set; }
}
