using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb
{
    public sealed class Constants
    {
        public const string DONATION_TRACKER_SHEET_ID = "1bwCvrcZYevyL0TwvTy5FiA58ce1vpU_bqqMtpGOfwrQ";
        public const int CACHE_NORMAL = 1800;
        public const int CACHE_MIN = 60;

        public const int WAR_SIZE1 = 40;
        public const int WAR_SIZE2 = 50;
        public const int CACHE_TIME = 5;
        public const double HIDE_TIME = 2.25;
        public const int PLAYER_BATCH = 200;

        public const int MIN_WEIGHT_CHANGES_ON_SUBMIT = 5;

        public const int WEIGHT_COMPARE = 30000;

        public const int MAXWEIGHT_TH14 = 140000;
        public const int MAXWEIGHT_TH13 = 130000;
        public const int MAXWEIGHT_TH12 = 120000;
        public const int MAXWEIGHT_TH11 = 110000;
        public const int MAXWEIGHT_TH10 = 90000;
        public const int MAXWEIGHT_TH9 = 70000;
        public const int MAXWEIGHT_TH8 = 55000;
        public const int MAXWEIGHT_TH7 = 40000;

        public static DateTime MaxVisibleSearchTime
        {
            get
            {
                return DateTime.UtcNow.AddHours(-2);
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

        public const string KUILIN_NET = "http://kuilin.net/cc_n";
        public const string CLASHOFSTATS = "https://www.clashofstats.com";
    }
}
