﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Models.ClanViewModels
{
    public class ClanIndexClan
    {
        public string Tag { get; set; }
        public string Name { get; set; }
        public int Members { get; set; }
        public string BadgeUrl { get; set; }
        public int WarCount { get; set; }
        public double MatchPercentage { get; set; }
        public double WinPercentage { get; set; }

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

        public SubmitRestriction SubmitRestriction { get; set; }
        public DateTime? SubmitRestrictionChangedAt { get; set; }
        public string SubmitRestrictionChangedByTag { get; set; }
        public string SubmitRestrictionChangedByName { get; set; }

        public DateTime WeightSubmitDate { get; set; }

        public bool PendingWeightSubmit { get; set; }

        public string SubmitRestrictionChangedByID
        {
            get
            {
                return Logic.Utils.TagToLinkId(SubmitRestrictionChangedByTag);
            }
        }

        public string LinkID
        {
            get
            {
                return Logic.Utils.TagToLinkId(Tag);
            }
        }
    }
}
