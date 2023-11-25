using FWAStatsWeb.Data;
using FWAStatsWeb.Logic;
using FWAStatsWeb.Models.DataViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FWAStatsWeb.Controllers
{
    //[ResponseCache(Duration = Constants.CACHE_NORMAL)]
    public class DataController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogger<DataController> logger;

        public DataController(
            ApplicationDbContext db,
            ILogger<DataController> logger)
        {
            this.db = db;
            this.logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [FormatFilter]
        [Route("Clan/{id}/Members.{format}")]
        public IActionResult ClanMembers(string id)
        {
            logger.LogInformation("Members {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var members = from m in db.Members
                          where m.ClanTag == tag
                          join p in db.Players on m.Tag equals p.Tag
                          join iw in db.Weights on m.Tag equals iw.Tag into Weights
                          from w in Weights.DefaultIfEmpty()
                          orderby m.ClanRank
                          select new { Member = m, Player = p, Weight = w };

            var data = new ClanMembers();

            foreach (var row in members)
            {
                data.Add(new ClanMemberModel
                {
                    Donated = row.Member.Donations,
                    InWar = row.Weight != null && row.Weight.InWar,
                    League = row.Member.LeagueName,
                    Level = row.Member.ExpLevel,
                    Name = row.Member.Name,
                    Rank = row.Member.ClanRank,
                    Received = row.Member.DonationsReceived,
                    Role = Utils.FixRoleName(row.Member.Role),
                    Tag = row.Member.Tag,
                    TownHall = row.Player.TownHallLevel,
                    Trophies = row.Member.Trophies,
                    Weight = row.Weight != null ? row.Weight.WarWeight : 0
                });
            }

            return Ok(data);
        }

        [FormatFilter]
        [Route("Clan/{id}/WarMembers.{format}")]
        public IActionResult WarMembers(string id, int warNo)
        {
            logger.LogInformation("WarMembers {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var maxPrepDate = Constants.MaxVisibleSearchTime;

            if (warNo <= 0)
                warNo = 1;

            var warId = db.Wars.Where(w => w.ClanTag == tag && w.PreparationStartTime < maxPrepDate).OrderByDescending(w => w.EndTime).Skip(warNo - 1).Select(w => w.ID).FirstOrDefault();

            var opponent = db.Wars.Where(w => w.ID == warId).Select(w => new { Tag = w.OpponentTag, Name = w.OpponentName }).FirstOrDefault();

            var members = from m in db.WarMembers
                          where m.WarID == warId && m.IsOpponent == false
                          join p in db.Players on m.Tag equals p.Tag
                          join iw in db.Weights on m.Tag equals iw.Tag into Weights
                          from w in Weights.DefaultIfEmpty()
                          orderby m.MapPosition
                          select new { Member = m, Player = p, Weight = w };

            var data = new WarMembers();

            foreach (var row in members)
            {
                data.Add(new WarMemberModel
                {
                    Position = row.Member.MapPosition,
                    Name = row.Member.Name,
                    Tag = row.Member.Tag,
                    TownHall = row.Player.TownHallLevel,
                    Weight = row.Weight != null ? row.Weight.WarWeight : 0,
                    Attacks = 0,
                    OpponentTag = opponent.Tag,
                    OpponentName = opponent.Name
                });
            }

            var attacks = from a in db.WarAttacks
                          where a.WarID == warId && a.IsOpponent == false
                          join o in db.WarMembers.Where(m => m.WarID == warId && m.IsOpponent == true) on a.DefenderTag equals o.Tag
                          orderby a.Order
                          select new { Attack = a, Opponent = o };

            foreach(var row in attacks)
            {
                var member = data.Where(d => d.Tag == row.Attack.AttackerTag).FirstOrDefault();
                if(member != null)
                {
                    member.Attacks++;
                    if (member.Attacks == 1)
                    {
                        member.Defender1Tag = row.Opponent.Tag;
                        member.Defender1Name = row.Opponent.Name;
                        member.Defender1Position = row.Opponent.MapPosition;
                        member.Defender1TownHall = row.Opponent.TownHallLevel;
                        member.Stars1 = row.Attack.Stars;
                        member.DestructionPercentage1 = row.Attack.DestructionPercentage;
                    }
                    else if (member.Attacks == 2)
                    {
                        member.Defender2Tag = row.Opponent.Tag;
                        member.Defender2Name = row.Opponent.Name;
                        member.Defender2Position = row.Opponent.MapPosition;
                        member.Defender2TownHall = row.Opponent.TownHallLevel;
                        member.Stars2 = row.Attack.Stars;
                        member.DestructionPercentage2 = row.Attack.DestructionPercentage;
                    }
                }
            }

            return Ok(data);
        }

        [FormatFilter]
        [Route("Clan/{id}/Wars.{format}")]
        public IActionResult ClanWars(string id)
        {
            logger.LogInformation("Wars {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var data = new ClanWars();

            var wars = from w in db.Wars
                       where w.ClanTag == tag
                       orderby w.EndTime descending
                       select w;

            foreach (var row in wars)
            {
                data.Add(new ClanWarModel
                {
                    EndTime = RoundToDate(row.EndTime, DateTimeKind.Utc),
                    Result = row.Result,
                    TeamSize = row.TeamSize,
                    ClanTag = row.ClanTag,
                    ClanName = row.ClanName,
                    ClanLevel = row.ClanLevel,
                    ClanStars = row.ClanStars,
                    ClanDestructionPercentage = row.ClanDestructionPercentage,
                    ClanAttacks = row.ClanAttacks,
                    ClanExpEarned = row.ClanExpEarned,
                    OpponentTag = row.OpponentTag,
                    OpponentName = row.OpponentName,
                    OpponentLevel = row.OpponentLevel,
                    OpponentStars = row.OpponentStars,
                    OpponentDestructionPercentage = row.OpponentDestructionPercentage,
                    Synced = row.Synced,
                    Matched = row.Matched
                });
            }

            return Ok(data);
        }

        [FormatFilter]
        [Route("Clans.{format}")]
        public IActionResult Clans()
        {
            logger.LogInformation("Clans");

            var data = new Clans();

            var badges = from c in db.Clans
                         select new ClanModel
                         {
                             Description = c.Description,
                             Image = c.BadgeUrl,
                             IsWarLogPublic = c.IsWarLogPublic,
                             Level = c.ClanLevel,
                             Location = c.LocationName,
                             Losses = c.WarLosses,
                             Name = c.Name,
                             Points = c.ClanPoints,
                             RequiredTrophies = c.RequiredTrophies,
                             Tag = c.Tag,
                             Ties = c.WarTies,
                             Type = c.Type,
                             WarFrequency = c.WarFrequency,
                             Wins = c.WarWins,
                             WinStreak = c.WarWinStreak
                         };

            var weights = new WeightCalculator(db).Calculate().ToDictionary(w => w.Tag);

            foreach (var row in badges)
            {
                if (weights.TryGetValue(row.Tag, out WeightCalculator.Results weight))
                {
                    row.Th16Count = weight.Th16Count;
                    row.Th15Count = weight.Th15Count;
                    row.Th14Count = weight.Th14Count;
                    row.Th13Count = weight.Th13Count;
                    row.Th12Count = weight.Th12Count;
                    row.Th11Count = weight.Th11Count;
                    row.Th10Count = weight.Th10Count;
                    row.Th9Count = weight.Th9Count;
                    row.Th8Count = weight.Th8Count;
                    row.ThLowCount = weight.ThLowCount;
                    row.EstimatedWeight = weight.EstimatedWeight;
                }

                data.Add(row);
            }

            return Ok(data);
        }

        [FormatFilter]
        [Route("Weights.{format}")]
        public IActionResult Weights()
        {
            logger.LogInformation("Weights");

            var data = new Weights();

            var weightQ = from w in db.Weights
                          where w.WarWeight > 0
                          join p in db.Players
                          on w.Tag equals p.Tag
                          select new WeightModel
                          {
                              Tag = w.Tag,
                              Weight = w.WarWeight,
                              Townhall = p.TownHallLevel,
                              LastModified = RoundToSec(w.LastModified, DateTimeKind.Utc)
                          };

            data.AddRange(weightQ);

            return Ok(data);
        }

        [FormatFilter]
        [Route("WeightUpdates.{format}")]
        public IActionResult WeightUpdates()
        {
            logger.LogInformation("WeightUpdates");

            var data = new Weights();

            var limitDate = DateTime.UtcNow.AddDays(-28);

            var weightQ = from w in db.Weights
                          where w.WarWeight > 0
                          && w.WarWeight != w.ExtWeight
                          && w.LastModified > limitDate
                          join p in db.Players
                          on w.Tag equals p.Tag
                          orderby w.LastModified
                          select new WeightModel
                          {
                              Tag = w.Tag,
                              Weight = w.WarWeight,
                              Townhall = p.TownHallLevel,
                              LastModified = RoundToSec(w.LastModified, DateTimeKind.Utc)
                          };

            data.AddRange(weightQ);

            return Ok(data);
        }

        protected static DateTime RoundToSec(DateTime input, DateTimeKind kind)
        {
            return new DateTime(input.Year, input.Month, input.Day, input.Hour, input.Minute, input.Second, kind);
        }

        protected static DateTime RoundToDate(DateTime input, DateTimeKind kind)
        {
            return new DateTime(input.Year, input.Month, input.Day, 0, 0, 0, kind);
        }
    }
}
