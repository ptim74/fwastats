using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class WeightDatabaseOptions
    {
        public string SheetId { get; set; }
        public string Range { get; set; }
        public int TagColumn { get; set; }
        public int WeightColumn { get; set; }
        public int TimestampColumn { get; set; }
    }
}
