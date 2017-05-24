using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LWFStatsWeb.Models.DataViewModels
{
    [XmlRoot("Weights")]
    public class Weights : List<WeightModel>
    {
    }

    [XmlType("Weight")]
    public class WeightModel
    {
        public string Tag { get; set; }
        public int Weight { get; set; }
        public DateTime LastModified { get; set; }
    }
}
