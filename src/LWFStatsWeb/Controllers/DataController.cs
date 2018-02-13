using LWFStatsWeb.Data;
using LWFStatsWeb.Logic;
using LWFStatsWeb.Models.DataViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LWFStatsWeb.Controllers
{
    [ResponseCache(Duration = Constants.CACHE_NORMAL)]
    public class DataController : Controller
    {
        private readonly ApplicationDbContext db;
        ILogger<DataController> logger;

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
                    InWar = row.Weight != null ? row.Weight.InWar : false,
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
        public IActionResult WarMembers(string id)
        {
            logger.LogInformation("WarMembers {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var maxEndDate = DateTime.UtcNow.AddDays(1);
            var warId = (from w in db.Wars where w.ClanTag == tag join m in db.WarMembers on w.ID equals m.WarID where w.EndTime < maxEndDate select w.ID).Max();

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
                    Weight = row.Weight != null ? row.Weight.WarWeight : 0
                });
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
                    EndTime = new DateTime(row.EndTime.Ticks, DateTimeKind.Utc).Date,
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

            foreach (var weight in db.Weights.Where(w => w.WarWeight > 0))
            {
                data.Add(new WeightModel
                {
                    Tag = weight.Tag,
                    Weight = weight.WarWeight,
                    LastModified = weight.LastModified
                });
            }

            return Ok(data);
        }

        [FormatFilter]
        [Route("WeightUpdates.{format}")]
        public IActionResult WeightUpdates()
        {
            logger.LogInformation("WeightUpdates");

            var data = new Weights();

            var limitDate = DateTime.UtcNow.AddDays(-28);

            foreach (var weight in db.Weights.Where(w => w.WarWeight > 0 && w.WarWeight != w.ExtWeight && w.LastModified > limitDate).OrderBy(w => w.LastModified))
            {
                data.Add(new WeightModel
                {
                    Tag = weight.Tag,
                    Weight = weight.WarWeight,
                    LastModified = weight.LastModified
                });
            }

            return Ok(data);
        }
    }
}
