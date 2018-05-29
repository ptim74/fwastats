using LWFStatsWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb
{
    public static class Extensions
    {
        /*
        public static DateTime EarliestPrepTime(this DbSet<War> wars)
        {
            return wars.Where(w => w.PreparationStartTime > DateTime.MinValue).Select(w => w.PreparationStartTime).Min();
        }
        */

        /*
        public static DateTime LatestEmptyPrepTime(this DbSet<War> wars)
        {
            return wars.Where(w => w.PreparationStartTime == DateTime.MinValue).Select(w => w.EndTime).Max();
        }*/
    }
}
