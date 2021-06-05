using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Logic
{
    public static class Utils
    {
        public static string LinkIdToTag(string id)
        {
            if (string.IsNullOrEmpty(id))
                return String.Empty;
            return string.Concat("#", id.Replace("#", "").ToUpperInvariant().Replace("O","0"));
        }

        public static string TagToLinkId(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return String.Empty;
            return tag.Replace("#", "");
        }

        public static string TimeSpanToString(TimeSpan timeSpan)
        {
            if (timeSpan > TimeSpan.FromDays(3650))
                return "never";
            else if (timeSpan > TimeSpan.FromDays(10))
                return string.Format("{0}d ago", timeSpan.Days);
            else if (timeSpan > TimeSpan.FromDays(1))
                return string.Format("{0}d {1}h ago", timeSpan.Days, timeSpan.Hours);
            else if (timeSpan > TimeSpan.FromHours(1))
                return string.Format("{0}h {1}m ago", timeSpan.Hours, timeSpan.Minutes);
            else if (timeSpan > TimeSpan.FromMinutes(2))
                return string.Format("{0}m ago", timeSpan.Minutes);
            else
                return "just now";
        }



        
        public static string FixRoleName(string roleName)
        {
            switch(roleName)
            {
                case "leader":
                    return "Leader";
                case "coLeader":
                    return "Co-leader";
                case "admin":
                    return "Elder";
                case "member":
                    return "Member";
                default:
                    return "Unknown";
            }
        }

        private static DateTime D = new DateTime(2017, 1, 1);

        public static long WarTimeToId(DateTime warDate)
        {
            return (long)Math.Floor(warDate.Subtract(D).TotalSeconds);
        }

        public static DateTime WarIdToTime(long id)
        {
            return D.AddSeconds(id);
        }

        private static TimeZoneInfo _easternTimeZone = null;

        public static TimeZoneInfo EasternTimeZone
        {
            get
            {
                if (_easternTimeZone == null)
                {
                    try
                    {
                        //Windows
                        _easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
                    }
                    catch (Exception)
                    {
                        //Linux
                        _easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("US/Eastern");
                    }
                }
                return _easternTimeZone;
            }
        }

        public static DateTime AsEasternTime(DateTime dateTime)
        {
            //round to second to eliminate unnecessary updates
            var ret = new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second);
            ret = ret.Subtract(Utils.EasternTimeZone.GetUtcOffset(ret));
            return ret;
        }
    }
}
