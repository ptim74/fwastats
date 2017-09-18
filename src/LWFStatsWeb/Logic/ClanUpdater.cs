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
                var task = new UpdateTask
                {
                    ID = Guid.NewGuid(),
                    ClanTag = oldItem.Key,
                    ClanName = oldItem.Value.Name
                };
                if (newList.TryGetValue(oldItem.Key, out ClanObject newItem))
                {
                    task.Mode = UpdateTaskMode.Update;
                    task.ClanGroup = newItem.Group;
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
                    var task = new UpdateTask
                    {
                        ID = Guid.NewGuid(),
                        ClanTag = newItem.Key,
                        ClanName = newItem.Value.Name,
                        ClanGroup = newItem.Value.Group,
                        Mode = UpdateTaskMode.Insert
                    };
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
