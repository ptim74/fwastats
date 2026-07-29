using System.Runtime.Serialization;

namespace FWAStatsWeb.Models;

[DataContract]
public class Location
{
    [DataMember]
    public int Id { get; set; }

    [DataMember]
    public string Name { get; set; }

    [DataMember]
    public Boolean IsCountry { get; set; }
}
