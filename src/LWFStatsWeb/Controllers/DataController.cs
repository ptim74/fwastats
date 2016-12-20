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
        ILogger<HomeController> logger;

        public DataController(
            ApplicationDbContext db,
            IMemoryCache memoryCache,
            ILogger<HomeController> logger)
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
            var tag = Utils.LinkIdToTag(id);

            var model = memoryCache.GetOrCreate("ClanMembers." + tag, entry =>
             {
                 entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

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
    }
}
