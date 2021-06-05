using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    [DataContract]
    public class Warlog
    {
        [DataMember(Name = "items")]
        public ICollection<War> Wars { get; set; }
    }
}
