using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LWFStatsWeb.Models;
using LWFStatsWeb.Models.UpdateViewModels;
using LWFStatsWeb.Data;
using LWFStatsWeb.Logic;
using Microsoft.EntityFrameworkCore;

namespace LWFStatsWeb.Controllers
{
    public class UpdateController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly IClanLoader loader;
        private readonly IClanUpdater updater;
        private readonly IClashApi api;
        private readonly IClanStatistics statistics;

        public UpdateController(
            ApplicationDbContext context, 
            IClanLoader loader,
            IClanUpdater updater,
            IClashApi api,
            IClanStatistics statistics)
        {
            this.db = context;
            this.loader = loader;
            this.updater = updater;
            this.api = api;
            this.statistics = statistics;
        }

        protected async Task<IndexViewModel> GetUpdates()
        {
            var model = new IndexViewModel();

            var loadedClans = await loader.Load();

            model.Errors = loader.Errors;

            model.Tasks = updater.GetUpdates(loadedClans);

            model.Tasks = model.Tasks.OrderBy(c => c.ClanName).ToList();

            return model;
        }

        protected async void DeleteTasks()
        {
            foreach(var task in db.UpdateTasks)
            {
                db.UpdateTasks.Remove(task);
            }

            await db.SaveChangesAsync();
        }

        protected async Task<UpdateTaskResponse> PerformTask(string id)
        {
            var status = new UpdateTaskResponse();
            status.ID = id;

            string clanName = "";
            UpdateTaskMode updateMode = UpdateTaskMode.Update;

            try
            {
                var task = db.UpdateTasks.Single(t => t.ID == Guid.Parse(id));

                clanName = task.ClanName;
                updateMode = task.Mode;

                db.UpdateTasks.Remove(task);
                db.SaveChanges();

                if (task.Mode == UpdateTaskMode.Delete)
                {
                    var clan = db.Clans.Single(c => c.Tag == task.ClanTag);
                    if (clan != null)
                    {
                        var members = db.Members.Where(m => m.ClanTag == clan.Tag);

                        foreach(var member in members.ToList())
                            db.Entry(member).State = EntityState.Deleted;
 
                        db.Entry(clan).State = EntityState.Deleted;
                        db.SaveChanges();
                    }
                }

                if (task.Mode == UpdateTaskMode.Update)
                {
                    var clan = await api.GetClan(task.ClanTag, true);

                    clan.Group = task.ClanGroup;

                    var existingClanQ = from c in db.Clans
                                        where c.Tag == task.ClanTag
                                        select new
                                        {
                                            BadgeUrl = c.BadgeUrl,
                                            ClanLevel = c.ClanLevel,
                                            ClanPoints = c.ClanPoints,
                                            ClanType = c.Type,
                                            Description = c.Description,
                                            Group = c.Group,
                                            IsWarLogPublic = c.IsWarLogPublic,
                                            MemberCount = c.Members,
                                            Name = c.Name,
                                            RequiredTrophies = c.RequiredTrophies,
                                            WarLosses = c.WarLosses,
                                            WarTies = c.WarTies,
                                            WarWins = c.WarWins,
                                            WarWinStreak = c.WarWinStreak
                                        };

                    var existingClan = existingClanQ.ToList().First();

                    var clanModified = false;

                    if (clan.BadgeUrl != existingClan.BadgeUrl)
                        clanModified = true;
                    if (clan.ClanLevel != existingClan.ClanLevel)
                        clanModified = true;
                    if (clan.ClanPoints != existingClan.ClanPoints)
                        clanModified = true;
                    if (clan.Type != existingClan.ClanType)
                        clanModified = true;
                    if (clan.Description != existingClan.Description)
                        clanModified = true;
                    if (clan.Group != existingClan.Group)
                        clanModified = true;
                    if (clan.IsWarLogPublic != existingClan.IsWarLogPublic)
                        clanModified = true;
                    if (clan.Members != existingClan.MemberCount)
                        clanModified = true;
                    if (clan.Name != existingClan.Name)
                        clanModified = true;
                    if (clan.RequiredTrophies != existingClan.RequiredTrophies)
                        clanModified = true;
                    if (clan.WarLosses != existingClan.WarLosses)
                        clanModified = true;
                    if (clan.WarTies != existingClan.WarTies)
                        clanModified = true;
                    if (clan.WarWins != existingClan.WarWins)
                        clanModified = true;
                    if (clan.WarWinStreak != existingClan.WarWinStreak)
                        clanModified = true;

                    if (clanModified)
                        db.Entry(clan).State = EntityState.Modified;

                    var allMembers = (from m in db.Members select m.Tag).ToList();

                    var oldMembers = (from m in db.Members
                                      where m.ClanTag == clan.Tag
                                      select new
                                      {
                                          Tag = m.Tag,
                                          BadgeUrl = m.BadgeUrl,
                                          ClanRank = m.ClanRank,
                                          ClanTag = m.ClanTag,
                                          Donations = m.Donations,
                                          DonationsReceived = m.DonationsReceived,
                                          ExpLevel = m.ExpLevel,
                                          Name = m.Name,
                                          Role = m.Role,
                                          Trophies = m.Trophies
                                      }).ToList();

                    if (clan.MemberList != null)
                    {
                        foreach (var clanMember in clan.MemberList.ToList())
                        {
                            var oldMember1 = from c in oldMembers where c.Tag == clanMember.Tag select c;
                            if (oldMember1.Count() > 0)
                            {
                                var oldMember = oldMember1.First();
                                var modified = false;
                                if (clanMember.BadgeUrl != oldMember.BadgeUrl)
                                    modified = true;
                                if (clanMember.ClanRank != oldMember.ClanRank)
                                    modified = true;
                                if (clanMember.ClanTag != oldMember.ClanTag)
                                    modified = true;
                                if (clanMember.Donations != oldMember.Donations)
                                    modified = true;
                                if (clanMember.DonationsReceived != oldMember.DonationsReceived)
                                    modified = true;
                                if (clanMember.ExpLevel != oldMember.ExpLevel)
                                    modified = true;
                                if (clanMember.Name != oldMember.Name)
                                    modified = true;
                                if (clanMember.Role != oldMember.Role)
                                    modified = true;
                                if (clanMember.Trophies != oldMember.Trophies)
                                    modified = true;
                                if (modified)
                                {
                                    db.Entry(clanMember).State = EntityState.Modified;
                                }
                            }
                            else
                            {
                                if (allMembers.Contains(clanMember.Tag))
                                    db.Entry(clanMember).State = EntityState.Modified;
                                else
                                    db.Entry(clanMember).State = EntityState.Added;
                            }
                        }

                        foreach (var clanMember in oldMembers)
                        {
                            var existingMember = (from m in clan.MemberList where m.Tag == clanMember.Tag select m).ToList();
                            if (existingMember.Count() == 0)
                            {
                                var formerMember = new Member { Tag = clanMember.Tag };
                                db.Entry(formerMember).State = EntityState.Deleted;
                            }
                        }

                    }

                    if (clan.Wars != null)
                    {
                        var clanWars = (from w in db.Wars where w.ClanTag == clan.Tag select w.ID).ToList();

                        foreach (var war in clan.Wars.ToList())
                        {
                            if (!clanWars.Contains(war.ID))
                            {
                                db.Entry(war).State = EntityState.Added;
                            }
                        }
                    }

                    db.SaveChanges();
                }

                if (task.Mode == UpdateTaskMode.Insert)
                {
                    var clan = await api.GetClan(task.ClanTag,true);
                    if (clan == null)
                        throw new Exception(string.Format("Clan {0} not found", task.ClanTag));

                    clan.Group = task.ClanGroup;

                    var existingMembers = (from m in db.Members where m.ClanTag != task.ClanTag select m.Tag).ToList();

                    foreach (var clanMember in clan.MemberList)
                    {
                        //Member hopped from other clan
                        if (existingMembers.Contains(clanMember.Tag))
                            db.Entry(clanMember).State = EntityState.Modified;
                    }

                    if (clan.Wars != null)
                    {
                        var existingWars = (from w in db.Wars where w.ClanTag == task.ClanTag select w.ID).ToList();

                        foreach (var clanWar in clan.Wars)
                        {
                            if (!existingWars.Contains(clanWar.ID))
                                db.Wars.Add(clanWar);
                        }
                    }

                    db.Clans.Add(clan);

                    db.SaveChanges();
                }

                status.Message = clanName;
                status.Status = true;
            }
            catch (Exception e)
            {
                var innerMessage = "";
                if (e.InnerException != null)
                    innerMessage = e.InnerException.Message;
                status.Message = string.Format("{0} {1} Failed: {2}, {3}", clanName, updateMode, e.Message, innerMessage);
                status.Status = false;
            }

            return status;
        }

        // GET: Update
        public async Task<IActionResult> Index()
        {
            IndexViewModel model = null;
            try
            {
                db.Database.Migrate();
                model = await GetUpdates();
            }
            catch (Exception e)
            {
                model = new IndexViewModel();
                model.Errors = new List<string>();
                model.Errors.Add(e.GetType().FullName);
                model.Errors.Add(e.Message);
                model.Errors.Add(e.StackTrace);
                model.Tasks = new List<UpdateTask>();
            }

            return View(model);
        }

        public async Task<IActionResult> GetTasks()
        {
            return Json(await GetUpdates());
        }

        public async Task<IActionResult> UpdateTask(string id)
        {
            try
            {
                return Json(await PerformTask(id));
            }
            catch(Exception e)
            {
                return Json(new UpdateTaskResponse { ID = id, Message = e.Message, Status = false });
            }
        }

        protected void PerformFinished()
        {
            this.DeleteTasks();
            statistics.UpdateValidities();
            statistics.CalculateSyncs();
            statistics.UpdateSyncMatch();
        }

        // GET: Update
        public IActionResult Finish()
        {
            var model = new IndexViewModel();
            model.Errors = new List<string>();
            model.Tasks = new List<UpdateTask>();
            return View("Index", model);
        }

        public IActionResult UpdateFinished()
        {
            var status = new UpdateTaskResponse { Message = "Done", Status = true };

            try
            {
                this.PerformFinished();
            }
            catch(Exception e)
            {
                status.Message = e.Message;
                status.Status = false;
            }

            return Json(status);
        }
    }
}