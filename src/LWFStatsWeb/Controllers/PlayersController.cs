using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Models.PlayerViewModels;
using LWFStatsWeb.Data;
using Microsoft.EntityFrameworkCore;
using LWFStatsWeb.Models;
using Microsoft.Extensions.Caching.Memory;
using LWFStatsWeb.Logic;
using Microsoft.Extensions.Logging;

namespace LWFStatsWeb.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClashApi api;
        private IMemoryCache memoryCache;
        ILogger<PlayersController> logger;

        public PlayersController(
            ApplicationDbContext db,
            IClashApi api,
            IMemoryCache memoryCache,
            ILogger<PlayersController> logger)
        {
            this.db = db;
            this.api = api;
            this.memoryCache = memoryCache;
            this.logger = logger;
        }

        public IActionResult Index(string q)
        {
            logger.LogInformation("Index {0}", q);

            var model = new SearchViewModel();

            if(!string.IsNullOrEmpty(q))
            {
                model.Query = q;
                model.Results = new List<SearchResultModel>();

                var playerTag = Utils.LinkIdToTag(q);
                var player1 = db.Players.Select(p => new SearchResultModel {
                    Tag = p.Tag,
                    Name = p.Name,
                    LastSeen = p.LastUpdated
                }).SingleOrDefault(p => p.Tag == playerTag);

                if(player1 != null)
                {
                    var member = db.Members.Where(m => m.Tag == player1.Tag).SingleOrDefault();
                    if(member != null)
                    {
                        player1.ClanTag = member.ClanTag;
                        var clan = db.Clans.Where(c => c.Tag == member.ClanTag).SingleOrDefault();
                        if(clan != null)
                        {
                            player1.ClanName = clan.Name;
                        }
                    }
                    model.Results.Add(player1);
                }
                else
                {
                    var clanNames = db.Clans.ToDictionary(c => c.Tag, c => c.Name);

                    var players = from p in db.Players
                                  where p.Name.ToUpperInvariant().Contains(q.ToUpperInvariant())
                                  join im in db.Members on p.Tag equals im.Tag into InnerMembers
                                  from m in InnerMembers.DefaultIfEmpty()
                                  select new SearchResultModel
                                  {
                                      Tag = p.Tag,
                                      Name = p.Name,
                                      LastSeen = p.LastUpdated,
                                      ClanTag = m != null ? m.ClanTag : null
                                  };

                    foreach(var player in players.OrderBy(p => p.Name.ToUpperInvariant()).Take(100))
                    {
                        if (!string.IsNullOrEmpty(player.ClanTag) && clanNames.ContainsKey(player.ClanTag))
                            player.ClanName = clanNames[player.ClanTag];
                        model.Results.Add(player);
                    }
                }
            }

            return View(model);
        }

        [Route("Player/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            logger.LogInformation("Details {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var model = await memoryCache.GetOrCreateAsync<DetailsViewModel>("PlayerDetails." + tag, async entry => {

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                var ret = new DetailsViewModel();
                ret.Events = new List<PlayerDetailsEvent>();
                ret.Player = await api.GetPlayer(tag);

                var events = from e in db.PlayerEvents
                                 join v in db.ClanValidities on e.ClanTag equals v.Tag
                                 where e.PlayerTag == tag
                                 orderby e.EventDate descending
                                 select new { Event = e, Name = v.Name };

                foreach (var clanEvent in events.Take(100))
                {
                    var e = new PlayerDetailsEvent { Tag = clanEvent.Event.ClanTag, Name = clanEvent.Name, EventDate = clanEvent.Event.EventDate, EventType = clanEvent.Event.EventType, TimeDesc = clanEvent.Event.TimeDesc() };
                    if (e.EventType == PlayerEventType.Promote || e.EventType == PlayerEventType.Demote)
                    {
                        e.Value = clanEvent.Event.RoleName;
                    }
                    else
                    {
                        e.Value = clanEvent.Event.Value.ToString();
                    }
                    ret.Events.Add(e);
                }

                return ret;
            });

            return View(model);
        }
    }
}
