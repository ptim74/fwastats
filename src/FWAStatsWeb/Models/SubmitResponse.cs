using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models
{
    public class SubmitResponse
    {
        public bool Status { get; set; }
        public string Details { get; set; }
        public SubmitError Error { get; set; }

        public override string ToString()
        {
            var ret = Details ?? String.Empty;

            if(!Status && Error != null)
            {
                if (string.IsNullOrEmpty(ret))
                    ret = Error.ToString();
                else
                    ret = $"{ret}, {Error}";
            }

            return ret;
        }

    }
}
