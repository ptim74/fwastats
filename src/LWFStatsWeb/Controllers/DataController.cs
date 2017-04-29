using LWFStatsWeb.Data;
using LWFStatsWeb.Logic;
using LWFStatsWeb.Models.DataViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Controllers
{
    public class DataController : Controller
    {
        private readonly ApplicationDbContext db;
        private IMemoryCache memoryCache;
        ILogger<DataController> logger;

        public DataController(
            ApplicationDbContext db,
            IMemoryCache memoryCache,
            ILogger<DataController> logger)
        {
            this.db = db;
            this.memoryCache = memoryCache;
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

            var model = memoryCache.GetOrCreate("Data.ClanMembers." + tag, entry =>
            {
                 entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);

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

                 return data;
             });

            return Ok(model);
        }

        [FormatFilter]
        [Route("Clan/{id}/Wars.{format}")]
        public IActionResult ClanWars(string id)
        {
            logger.LogInformation("Wars {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var model = memoryCache.GetOrCreate("Data.ClanWars." + tag, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);

                var data = new ClanWars();

                var wars = from w in db.Wars
                           where w.ClanTag == tag
                           orderby w.EndTime descending
                           select w;

                foreach(var row in wars)
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

                return data;
            });

            return Ok(model);
        }

        [FormatFilter]
        [Route("Clans.{format}")]
        public IActionResult Clans()
        {
            logger.LogInformation("Clans");

            var model = memoryCache.GetOrCreate("Data.Clans", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(Constants.CACHE_TIME);

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
                    WeightCalculator.Results weight = null;
                    if (weights.TryGetValue(row.Tag, out weight))
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

                return data;
            });

            return Ok(model);
        }
    }
}
