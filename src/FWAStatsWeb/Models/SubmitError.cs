using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public class SubmitError
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public int LineNumber { get; set; }
        public string FileName { get; set; }

        public override string ToString()
        {
            return $"{Name}: {Message} (line {LineNumber} in '{FileName}')";
        }

    }
}
