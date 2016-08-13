using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Models.GraphViewModels;
using LWFStatsWeb.Data;

namespace LWFStatsWeb.Controllers
{
    public class GraphsController : Controller
    {
        private readonly ApplicationDbContext db;

        public GraphsController(ApplicationDbContext db)
        {
            this.db = db;
        }

        // GET: Graphs
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetData()
        {
            var model = new GraphNetwork();

            try
            {
                var earliestStart = DateTime.UtcNow;
                var recentWars = db.WarSyncs.OrderByDescending(w => w.Start).Take(10).ToList();
                foreach (var recentWar in recentWars)
                {
                    if (recentWar.Start < earliestStart)
                        earliestStart = recentWar.Start;
                }

                var clanList = db.Clans.ToDictionary(c => c.Tag);

                var edgesUsed = new List<string>();

                var graphClans = new Dictionary<string, GraphNode>();

                var wars = (from w in db.Wars
                            where w.EndTime >= earliestStart
                            select new
                            {
                                ClanTag = w.ClanTag,
                                OpponentTag = w.OpponentTag,
                                OpponentName = w.OpponentName
                            }).ToList();

                foreach (var war in wars)
                {
                    if (clanList.ContainsKey(war.OpponentTag) && clanList.ContainsKey(war.ClanTag))
                    {
                        var clanId = 0;
                        var opponentId = 0;

                        if (!graphClans.ContainsKey(war.ClanTag))
                        {
                            clanId = graphClans.Count();
                            graphClans.Add(war.ClanTag, new GraphNode { Id = clanId, Label = clanList[war.ClanTag].Name });
                        }
                        else
                        {
                            clanId = graphClans[war.ClanTag].Id;
                        }

                        if (!graphClans.ContainsKey(war.OpponentTag))
                        {
                            opponentId = graphClans.Count();
                            graphClans.Add(war.OpponentTag, new GraphNode { Id = opponentId, Label = clanList[war.OpponentTag].Name });
                        }
                        else
                        {
                            opponentId = graphClans[war.OpponentTag].Id;
                        }

                        var lowerId = clanId < opponentId ? clanId : opponentId;
                        var upperId = clanId > opponentId ? clanId : opponentId;

                        var edgeId = string.Format("{0}-{1}", lowerId, upperId);

                        if (!edgesUsed.Contains(edgeId))
                        {
                            model.Edges.Add(new GraphEdge { From = lowerId, To = upperId });
                            edgesUsed.Add(edgeId);
                        }
                    }
                }

                model.Nodes = graphClans.Values.ToList();
            }
            catch (Exception e)
            {
                model.Nodes.Add(new GraphNode { Id = -1, Label = e.Message });
            }

            return Json(model);
        }
    }
}