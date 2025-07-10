using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace FWAStatsWeb.Models.DataViewModels;

[XmlRoot("Weights")]
public class Weights : List<WeightModel>
{
}

[XmlType("Weight")]
public class WeightModel
{
    public string Tag { get; set; }
    public int Weight { get; set; }
    public int Townhall { get; set; }
    public DateTime LastModified { get; set; }
}
