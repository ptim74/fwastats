using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class BadgeUrls
{
    [DataMember]
    public string Small { get; set; }

    [DataMember]
    public string Medium { get; set; }

    [DataMember]
    public string Large { get; set; }
}
