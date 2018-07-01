using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class WeightDatabaseOptions
    {
        public string Url { get; set; }
        public string ResourceName { get; set; }
        public int SinceHours { get; set; }
        public string SheetId { get; set; }
        public string Range { get; set; }
        public int TagColumn { get; set; }
        public int WeightColumn { get; set; }
        public int TimestampColumn { get; set; }
    }
}
