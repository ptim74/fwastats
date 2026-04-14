using FWAStatsWeb.Data;
using FWAStatsWeb.Models.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FWAStatsWeb.Logic;

public class WeightCalculator
{
    public class Results
    {
        public string Tag { get; set; }
        public int Th18Count { get; set; }
        public int Th17Count { get; set; }
        public int Th16Count { get; set; }
        public int Th15Count { get; set; }
        public int Th14Count { get; set; }
        public int Th13Count { get; set; }
        public int Th12Count { get; set; }
        public int Th11Count { get; set; }
        public int Th10Count { get; set; }
        public int Th9Count { get; set; }
        public int Th8Count { get; set; }
        public int ThLowCount { get; set; }
        public int EstimatedWeight { get; set; }
    }

    private readonly ApplicationDbContext db;

    public WeightCalculator(ApplicationDbContext db)
    {
        this.db = db;
    }

    private static int MaxWeightForTH(int thLevel) => thLevel switch
    {
        <= 8  => Constants.MAXWEIGHT_TH8,
        9     => Constants.MAXWEIGHT_TH9,
        10    => Constants.MAXWEIGHT_TH10,
        11    => Constants.MAXWEIGHT_TH11,
        12    => Constants.MAXWEIGHT_TH12,
        13    => Constants.MAXWEIGHT_TH13,
        14    => Constants.MAXWEIGHT_TH14,
        15    => Constants.MAXWEIGHT_TH15,
        16    => Constants.MAXWEIGHT_TH16,
        17    => Constants.MAXWEIGHT_TH17,
        _     => Constants.MAXWEIGHT_TH18,
    };

    public IEnumerable<Results> Calculate()
    {
        // Subquery: latest war ID per clan, then join its members and outer join weights
        var latestWarIds = db.Wars
            .GroupBy(w => w.ClanTag)
            .Select(g => new { ClanTag = g.Key, WarID = g.Max(w => w.ID) });

        var warData = (from lw in latestWarIds
                       join wm in db.WarMembers on lw.WarID equals wm.WarID
                       where !wm.IsOpponent
                       join wt in db.Weights on wm.Tag equals wt.Tag into wtGroup
                       from wt in wtGroup.DefaultIfEmpty()
                       select new
                       {
                           lw.ClanTag,
                           MemberTag = wm.Tag,
                           wm.MapPosition,
                           wm.TownHallLevel,
                           WarWeight = wt != null ? wt.WarWeight : 0
                       })
                      .ToLookup(x => x.ClanTag);

        var qthlevelq = (from m in db.Members
                         join p in db.Players on m.Tag equals p.Tag
                         group m by new { m.ClanTag, p.TownHallLevel } into g
                         select new { g.Key.ClanTag, g.Key.TownHallLevel, Count = g.Count() })
                        .ToLookup(t => t.ClanTag);

        foreach (var thlevels in qthlevelq)
        {
            var ret = new Results { Tag = thlevels.Key };

            foreach (var th in thlevels)
            {
                switch (th.TownHallLevel)
                {
                    case 18: ret.Th18Count = th.Count; break;
                    case 17: ret.Th17Count = th.Count; break;
                    case 16: ret.Th16Count = th.Count; break;
                    case 15: ret.Th15Count = th.Count; break;
                    case 14: ret.Th14Count = th.Count; break;
                    case 13: ret.Th13Count = th.Count; break;
                    case 12: ret.Th12Count = th.Count; break;
                    case 11: ret.Th11Count = th.Count; break;
                    case 10: ret.Th10Count = th.Count; break;
                    case 9:  ret.Th9Count = th.Count; break;
                    case 8:  ret.Th8Count = th.Count; break;
                    default: ret.ThLowCount += th.Count; break;
                }
            }

            var clanData = warData[thlevels.Key];
            if (clanData.Any())
            {
                // Members are ordered top-to-bottom (position 1 = highest weight),
                // so iterate bottom-to-top and carry the last known weight upward
                // to fill in any missing (0) or outdated (lower-than-below) entries.
                var members = clanData
                    .OrderByDescending(m => m.MapPosition)
                    .ToList();

                int lastWeight = 0;
                int totalWeight = 0;

                foreach (var member in members)
                {
                    var maxWeight = MaxWeightForTH(member.TownHallLevel);
                    int weight = member.WarWeight == 0
                        ? maxWeight - 5000 // defaults to almost maxed weight for the TH level
                        : member.WarWeight;

                    // Carry up from below, but cap to current TH's maximum
                    lastWeight = weight = Math.Max(weight, Math.Min(lastWeight, maxWeight));

                    totalWeight += weight;
                }

                // Normalize to a 50-member war so weights are comparable across different war sizes
                if (members.Count > 0)
                    totalWeight = totalWeight * Constants.WAR_SIZE2 / members.Count;

                ret.EstimatedWeight = totalWeight / 1000;
            }

            yield return ret;
        }
    }
}
