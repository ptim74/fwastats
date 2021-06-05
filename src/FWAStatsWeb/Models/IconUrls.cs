using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    [DataContract]
    public class IconUrls
    {
        [DataMember]
        public string Small { get; set; }

        [DataMember]
        public string Medium { get; set; }

        [DataMember]
        public string Tiny { get; set; }
    }
}
