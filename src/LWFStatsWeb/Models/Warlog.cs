using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    [DataContract]
    public class Warlog
    {
        [DataMember(Name = "items")]
        public List<War> Wars { get; set; }
    }
}
