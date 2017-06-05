using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb
{
    public sealed class Constants
    {
        public const int CACHE_TIME = 15;
        public const double HIDE_TIME = 2.25;
        public const int PLAYER_BATCH = 200;

        public static DateTime MaxVisibleEndTime
        {
            get
            {
                return DateTime.UtcNow.AddHours(47.0 - HIDE_TIME);
            }
        }

        public const string CACHE_HOME_INDEX = "Home.Index";
        public const string CACHE_CLANS_ALL = "Clans.All";
        public const string CACHE_CLANS_DEPARTED = "Clans.Departed";

        public const string CACHE_CLANS_DETAILS_ = "ClanDetails.";
        public const string CACHE_CLANS_FOLLOWING = "Clans.Following";

        public const string CACHE_DATA_MEMBERS_ = "Data.ClanMembers.";
        public const string CACHE_DATA_WARS_ = "Data.ClanWars.";
        public const string CACHE_DATA_CLANS = "Data.Clans";
        public const string CACHE_DATA_WARMEMBERS_ = "Data.WarMembers.";
        public const string CACHE_DATA_WEIGHTS = "Data.Weights";

        public const string CACHE_PLAYER_DETAILS_ = "PlayerDetails.";

        public const string CACHE_SYNCS_ALL = "Syncs.All";
        public const string CACHE_SYNC_DETAILS_ = "Sync.Details.";

        public const string LIST_FWA = "FWA";
        public const string LIST_BLACKLISTED = "Blacklisted";

    }
}
