using LWFStatsWeb.Data;
using LWFStatsWeb.Logic;
using LWFStatsWeb.Models;
using LWFStatsWeb.Models.TrackerViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Controllers
{
    [ResponseCache(Duration = Constants.CACHE_MIN)]
    public class TrackerController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly ILogger<DataController> logger;
        private readonly ITrackerUtility trackerUtility;
        private readonly IOptions<TrackerOptions> options;

        public TrackerController(
            ApplicationDbContext db,
            ILogger<DataController> logger,
            ITrackerUtility trackerUtility,
            IOptions<TrackerOptions> options)
        {
            this.db = db;
            this.logger = logger;
            this.trackerUtility = trackerUtility;
            this.options = options;
        }

        public async Task<ActionResult> Callback(string code, string state)
        {
            var token = await trackerUtility.GetAccessToken(code);

            var tag = Utils.LinkIdToTag(state);

            var clanTracker = new ClanTracker
            {
                ClanTag = tag,
                ClientId = options.Value.ClientId,
                Id = token.Webhook.Id,
                Token = token.Webhook.Token
            };

            return Json($"code: {code}, state: {state}");
        }
    }
}
