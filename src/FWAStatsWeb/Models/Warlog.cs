using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class Warlog
{
    [DataMember(Name = "items")]
    public ICollection<War> Wars { get; set; }
}
