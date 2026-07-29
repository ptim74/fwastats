using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class WarLeague
{
    [DataMember]
    [StringLength(15)]
    public string State { get; set; } 

    [DataMember]
    [StringLength(15)]
    public string Season { get; set; }

    [DataMember]
    public virtual ICollection<PlayerClan> Clans { get; set; }
}
