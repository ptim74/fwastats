﻿using LWFStatsWeb.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class WeightCalculator
    {
        public class Results
        {
            public string Tag { get; set; }
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

        public IEnumerable<Results> Calculate()
        {
            var qthlevelq = (from m in db.Members
                     join p in db.Players on m.Tag equals p.Tag
                     group m by new { m.ClanTag, p.TownHallLevel } into g
                     select new { g.Key.ClanTag, g.Key.TownHallLevel, Count = g.Count() }).ToLookup(t => t.ClanTag);

            foreach(var thlevels in qthlevelq)
            {
                var ret = new Results { Tag = thlevels.Key };

                foreach (var th in thlevels)
                {
                    switch (th.TownHallLevel)
                    {
                        case 12:
                            ret.Th12Count = th.Count;
                            break;
                        case 11:
                            ret.Th11Count = th.Count;
                            break;
                        case 10:
                            ret.Th10Count = th.Count;
                            break;
                        case 9:
                            ret.Th9Count = th.Count;
                            break;
                        case 8:
                            ret.Th8Count = th.Count;
                            break;
                        default:
                            ret.ThLowCount += th.Count;
                            break;
                    }
                }

                //Calculate estimated max weight
                var availableMembers = Constants.WAR_SIZE1; //TODO

                var thLevelMembers = ret.Th12Count < availableMembers ? ret.Th12Count : availableMembers;
                var maxWeight = thLevelMembers * 125;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th11Count < availableMembers ? ret.Th11Count : availableMembers;
                maxWeight += thLevelMembers * 105;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th10Count < availableMembers ? ret.Th10Count : availableMembers;
                maxWeight += thLevelMembers * 85;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th9Count < availableMembers ? ret.Th9Count : availableMembers;
                maxWeight += thLevelMembers * 65;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th8Count < availableMembers ? ret.Th8Count : availableMembers;
                maxWeight += thLevelMembers * 50;
                availableMembers -= thLevelMembers;


                //Calculate estimated min weight
                availableMembers = Constants.WAR_SIZE1; //TODO
                thLevelMembers = ret.Th8Count < availableMembers ? ret.Th8Count : availableMembers;

                //TH8 count has limits
                if (thLevelMembers > 5)
                    thLevelMembers = 5;
                if (thLevelMembers > 1 && maxWeight > 3300)
                    thLevelMembers = 1;
                var th8Members = thLevelMembers;

                var minWeight = thLevelMembers * 50;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th9Count < availableMembers ? ret.Th9Count : availableMembers;
                minWeight += thLevelMembers * 65;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th10Count < availableMembers ? ret.Th10Count : availableMembers;
                minWeight += thLevelMembers * 85;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th11Count < availableMembers ? ret.Th11Count : availableMembers;
                minWeight += thLevelMembers * 105;
                availableMembers -= thLevelMembers;

                thLevelMembers = ret.Th12Count < availableMembers ? ret.Th12Count : availableMembers;
                minWeight += thLevelMembers * 125;
                availableMembers -= thLevelMembers;

                ret.EstimatedWeight = (maxWeight + minWeight) / 2;

                yield return ret;
            }

        }
    }
}
