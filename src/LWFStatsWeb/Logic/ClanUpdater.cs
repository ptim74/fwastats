using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public interface IClanUpdater
    {
        List<UpdateTask> GetUpdates(List<ClanObject> newClans);
    }

    public class ClanUpdater : IClanUpdater
    {
        private readonly ApplicationDbContext db;

        public ClanUpdater(ApplicationDbContext db)
        {
            this.db = db;
        }

        public List<UpdateTask> GetUpdates(List<ClanObject> newClans)
        {
            var tasks = new List<UpdateTask>();

            var newList = newClans.ToDictionary(c => c.Tag);

            var oldList = db.Clans.ToDictionary(c => c.Tag);

            foreach (var oldItem in oldList)
            {
                var task = new UpdateTask();
                task.ID = Guid.NewGuid();
                task.ClanTag = oldItem.Key;
                task.ClanName = oldItem.Value.Name;
                if (newList.ContainsKey(oldItem.Key))
                {
                    task.Mode = UpdateTaskMode.Update;
                    task.ClanGroup = newList[oldItem.Key].Group;
                }
                else
                {
                    task.Mode = UpdateTaskMode.Delete;
                }
                tasks.Add(task);
            }

            foreach (var newItem in newList)
            {
                if (!oldList.ContainsKey(newItem.Key))
                {
                    var task = new UpdateTask();
                    task.ID = Guid.NewGuid();
                    task.ClanTag = newItem.Key;
                    task.ClanName = newItem.Value.Name;
                    task.ClanGroup = newItem.Value.Group;
                    task.Mode = UpdateTaskMode.Insert;
                    tasks.Add(task);
                }
            }

            foreach (var task in tasks)
            {
                db.UpdateTasks.Add(task);
            }

            db.SaveChanges();

            return tasks;
        }
    }
}
