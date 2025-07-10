using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class League
{
    [DataMember]
    public int ID { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public IconUrls IconUrls { get; set; }
}
