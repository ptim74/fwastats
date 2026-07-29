using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class SubmitLog
{
    public int ID { get; set; }

    [DataMember]
    [StringLength(15)]
    public string Tag { get; set; }

    [DataMember]
    public DateTime Modified { get; set; }

    [DataMember]
    [StringLength(50)]
    public string IpAddr { get; set; }

    [DataMember]
    [StringLength(50)]
    public string Cookie { get; set; }

    [DataMember]
    public int Changes { get; set; }
}
