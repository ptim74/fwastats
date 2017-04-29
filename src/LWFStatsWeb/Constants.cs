using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb
{
    public sealed class Constants
    {
        public const int CACHE_TIME = 15;
        public const double HIDE_TIME = 2.5;
        public const int PLAYER_BATCH = 1000;

        public static DateTime MaxVisibleEndTime
        {
            get
            {
                return DateTime.UtcNow.AddHours(47.0 - HIDE_TIME);
            }
        }
    }
}
