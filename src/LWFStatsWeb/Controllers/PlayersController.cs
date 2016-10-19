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

namespace LWFStatsWeb.Controllers
{
    public class PlayersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClashApi api;
        private IMemoryCache memoryCache;

        public PlayersController(
            ApplicationDbContext db,
            IClashApi api,
            IMemoryCache memoryCache)
        {
            this.db = db;
            this.api = api;
            this.memoryCache = memoryCache;
        }

        public IActionResult Index(string q)
        {
            var model = new IndexViewModel();
            return View(model);
        }

        public async Task<IActionResult> Details(string id)
        {
            var tag = LinkIdToTag(id);

            var model = await memoryCache.GetOrCreateAsync<IndexViewModel>("PlayerDetails." + tag, async entry => {

                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                var ret = new IndexViewModel();
                ret.Events = new List<PlayerDetailsEvent>();
                ret.Player = await api.GetPlayer(tag);

                var events = from e in db.PlayerEvents
                                 join v in db.ClanValidities on e.ClanTag equals v.Tag
                                 where e.PlayerTag == tag
                                 orderby e.EventDate descending
                                 select new { Event = e, Name = v.Name };

                foreach (var clanEvent in events.Take(50))
                {
                    var e = new PlayerDetailsEvent { Tag = clanEvent.Event.ClanTag, Name = clanEvent.Name, EventDate = clanEvent.Event.EventDate, EventType = clanEvent.Event.EventType };
                    if (e.EventType == PlayerEventType.Promote || e.EventType == PlayerEventType.Demote)
                    {
                        e.Value = clanEvent.Event.Role;
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

        protected string LinkIdToTag(string id)
        {
            return string.Concat("#", id.Replace("#", ""));
        }

    }
}
