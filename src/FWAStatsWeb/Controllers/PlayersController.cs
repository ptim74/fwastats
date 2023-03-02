using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using FWAStatsWeb.Models.PlayerViewModels;
using FWAStatsWeb.Data;
using FWAStatsWeb.Models;
using FWAStatsWeb.Logic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace FWAStatsWeb.Controllers
{
    //[ResponseCache(Duration = Constants.CACHE_MIN)]
    public class PlayersController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClashApi api;
        private readonly IMemberUpdater memberUpdater;
        private readonly ILogger<PlayersController> logger;
        private readonly UserManager<ApplicationUser> userManager;

        public PlayersController(
            ApplicationDbContext db,
            IClashApi api,
            IMemberUpdater memberUpdater,
            ILogger<PlayersController> logger,
            UserManager<ApplicationUser> userManager)
        {
            this.db = db;
            this.api = api;
            this.memberUpdater = memberUpdater;
            this.logger = logger;
            this.userManager = userManager;
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

                    #pragma warning disable IDE0031
                    var players = from p in db.Players
                                  where p.Name.ToUpper().Contains(q.ToUpper())
                                  join im in db.Members on p.Tag equals im.Tag into InnerMembers
                                  from m in InnerMembers.DefaultIfEmpty()
                                  select new SearchResultModel
                                  {
                                      Tag = p.Tag,
                                      Name = p.Name,
                                      LastSeen = p.LastUpdated,
                                      ClanTag = (m != null ? m.ClanTag : null)
                                  };
                    #pragma warning restore IDE0031

                    foreach (var player in players.OrderBy(p => p.Name.ToUpper()).Take(100))
                    {
                        if (!string.IsNullOrEmpty(player.ClanTag) && clanNames.ContainsKey(player.ClanTag))
                            player.ClanName = clanNames[player.ClanTag];
                        model.Results.Add(player);
                    }
                }
            }

            return View(model);
        }

        [Authorize]
        public async Task<IActionResult> My()
        {
            logger.LogInformation("My");

            var user = await GetCurrentUserAsync();

            var myPlayers =  new List<MyPlayerModel>();

            var playerQ = db.PlayerClaims.Where(p => p.UserId == user.Id);

            foreach(var player in playerQ)
            {
                var myPlayerQ = from m in db.Members
                                where m.Tag == player.Tag
                                join c in db.Clans on m.ClanTag equals c.Tag
                                select new MyPlayerModel
                                {
                                    Name = m.Name,
                                    Tag = m.Tag,
                                    ClanName = c.Name,
                                    ClanTag = c.Tag,
                                    ClanSubmitRestriction = c.SubmitRestriction,
                                    IsFWA = true,
                                };

                var myPlayer = myPlayerQ.SingleOrDefault();

                if(myPlayer == null)
                {
                    try
                    {
                        var newPlayer = await api.GetPlayer(player.Tag);
                        myPlayer = new MyPlayerModel
                        {
                            Name = newPlayer.Name,
                            Tag = newPlayer.Tag,
                            ClanName = newPlayer.ClanName,
                            ClanTag = newPlayer.ClanTag,
                            IsFWA = false
                        };
                    } 
                    catch (Exception)
                    {
                        myPlayer = new MyPlayerModel
                        {
                            Name = player.Tag,
                            Tag = player.Tag,
                            IsError = true,
                            IsFWA = false
                        };
                    }
                }

                myPlayers.Add(myPlayer);
            }

            var model = new MyPlayersViewModel { Players = myPlayers.OrderBy(p => p.Name).ToList() };

            return View(model);
        }


        [Route("Player/{id}")]
        public async Task<IActionResult> Details(string id)
        {
            logger.LogInformation("Details {0}", id);

            var tag = Utils.LinkIdToTag(id);

            var ret = new DetailsViewModel
            {
                Events = new List<PlayerDetailsEvent>(),
                Player = await api.GetPlayer(tag)
            };

            var user = await GetCurrentUserAsync();
            if(user != null)
            {
                var userClaim = db.PlayerClaims.FirstOrDefault(p => p.Tag == tag);
                if(userClaim != null && userClaim.UserId == user.Id)
                {
                    ret.Claimed = true;
                }
            }

            memberUpdater.UpdatePlayer(ret.Player, !ret.Claimed);

            var events = from e in db.PlayerEvents
                         join v in db.ClanValidities on e.ClanTag equals v.Tag
                         where e.PlayerTag == tag
                         orderby e.EventDate descending
                         select new { Event = e, v.Name };

            foreach (var clanEvent in events.Take(100))
            {
                var e = new PlayerDetailsEvent { Tag = clanEvent.Event.ClanTag, Name = clanEvent.Name, EventDate = clanEvent.Event.EventDate, EventType = clanEvent.Event.EventType, TimeDesc = clanEvent.Event.TimeDesc() };
                if (e.EventType == PlayerEventType.Promote || e.EventType == PlayerEventType.Demote)
                {
                    e.Value = clanEvent.Event.RoleName;
                }
                else if (e.EventType == PlayerEventType.NameChange)
                {
                    e.Value = clanEvent.Event.StringValue;
                }
                else
                {
                    e.Value = clanEvent.Event.Value.ToString();
                }
                ret.Events.Add(e);
            }

            return View(ret);
        }

        [Authorize]
        [HttpGet]
        [Route("Players/Link")]
        public IActionResult NewLink()
        {
            var model = new LinkViewModel();
            return View("Link", model);
        }

        [Authorize]
        [HttpGet]
        [Route("Player/{id}/Link")]
        public IActionResult Link(string id)
        {
            var tag = Utils.LinkIdToTag(id);
            var model = new LinkViewModel { Tag = tag };
            return View(model);
        }

        [Authorize]
        [HttpGet]
        [Route("Player/{id}/Unlink")]
        public IActionResult Unlink(string id)
        {
            var tag = Utils.LinkIdToTag(id);
            var model = new UnlinkViewModel { Tag = tag };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateLink(LinkViewModel model)
        {
            if (ModelState.IsValid)
            {
                logger.LogInformation("Link {0}", model.Tag);

                var tag = Utils.LinkIdToTag(model.Tag);

                var user = await GetCurrentUserAsync();
                if(user == null)
                {
                    ModelState.AddModelError(string.Empty, "Not Logged in");
                }
                else
                {
                    var status = await api.VerifyPlayer(tag, model.ApiToken);
                    if (status != null && status == "ok")
                    {
                        var claim = db.PlayerClaims.FirstOrDefault(p => p.Tag == tag);
                        if (claim == null)
                        {
                            claim = new PlayerClaim
                            {
                                UserId = user.Id,
                                Tag = tag
                            };
                            db.PlayerClaims.Add(claim);
                        }
                        else
                        {
                            claim.UserId = user.Id;
                        }
                        db.SaveChanges();
                        var player = db.Players.FirstOrDefault(p => p.Tag == tag);
                        if(player == null)
                        {
                            player = await api.GetPlayer(tag);
                            memberUpdater.UpdatePlayer(player, true);
                        }
                        return RedirectToAction(nameof(My));
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Linking player failed. Please check API Key.");
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(nameof(Link), model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteLink(UnlinkViewModel model)
        {
            if (ModelState.IsValid)
            {
                logger.LogInformation("Unlink {0}", model.Tag);

                var tag = Utils.LinkIdToTag(model.Tag);

                var user = await GetCurrentUserAsync();
                if (user == null)
                {
                    ModelState.AddModelError(string.Empty, "Not Logged in");
                }
                else
                {
                    var claim = db.PlayerClaims.FirstOrDefault(p => p.Tag == tag && p.UserId == user.Id);
                    if(claim == null)
                    {
                        ModelState.AddModelError(string.Empty, "Link not found");
                    }
                    else
                    {
                        db.PlayerClaims.Remove(claim);
                        db.SaveChanges();
                        return RedirectToAction(nameof(My));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(nameof(Link), model);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return userManager.GetUserAsync(HttpContext.User);
        }
    }
}
