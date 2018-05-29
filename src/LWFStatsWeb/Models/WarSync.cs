using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models
{
    public class WarSync
    {
        public int ID { get; set; }
        public DateTime Start { get; set; }
        public DateTime Finish { get; set; }
        public int AllianceMatches { get; set; }
        public int WarMatches { get; set; }
        public int MissedStarts { get; set; }
        public bool Verified { get; set; }

        public string DisplayName
        {
            get
            {
                return Start.ToString("yyyy-MM-dd");
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return Finish.Subtract(Start);
            }
        }

        public bool IsStarted
        {
            get
            {
                return Start != DateTime.MinValue;
            }
        }

        public bool IsFinished
        {
            get
            {
                return Finish != DateTime.MinValue;
            }
        }
    }
}
