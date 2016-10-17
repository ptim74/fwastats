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

            var model = await memoryCache.GetOrCreateAsync<Player>("PlayerDetails." + tag, async entry => {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);

                return await api.GetPlayer(tag);
            });

            return View(model);
        }

        protected string LinkIdToTag(string id)
        {
            return string.Concat("#", id.Replace("#", ""));
        }

    }
}
