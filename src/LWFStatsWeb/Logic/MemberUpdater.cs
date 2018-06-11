using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class MemberUpdateResponse
    {
        public string Message { get; set; }
        public bool Status { get; set; }
    }

    public interface IMemberUpdater
    {
        MemberUpdateResponse UpdatePlayer(Player newPlayer, bool updateOnly);
    }

    public class MemberUpdater : IMemberUpdater
    {
        private readonly ApplicationDbContext db;

        public MemberUpdater(
            ApplicationDbContext context)
        {
            this.db = context;
        }

        public MemberUpdateResponse UpdatePlayer(Player newPlayer, bool updateOnly = false)
        {
            var status = new MemberUpdateResponse();
            var playerTag = newPlayer.Tag;
            var playerName = newPlayer.Tag;

            try
            {
                playerName = $"{newPlayer.Name} / {newPlayer.ClanName}";
                var oldPlayer = db.Players.SingleOrDefault(e => e.Tag == playerTag);
                if (oldPlayer == null)
                {
                    if (!updateOnly)
                    {
                        newPlayer.LastUpdated = DateTime.UtcNow;
                        db.Entry(newPlayer).State = EntityState.Added;
                        db.SaveChanges();
                    }
                }
                else
                {
                    if (oldPlayer.TownHallLevel != newPlayer.TownHallLevel)
                    {
                        db.Add(new PlayerEvent
                        {
                            ClanTag = newPlayer.ClanTag,
                            PlayerTag = newPlayer.Tag,
                            EventDate = DateTime.UtcNow,
                            EventType = PlayerEventType.Townhall,
                            Value = newPlayer.TownHallLevel
                        });
                    }

                    if (oldPlayer.Name != newPlayer.Name)
                    {
                        var oldMember = db.Members.SingleOrDefault(m => m.Tag == playerTag);
                        if (oldMember != null)
                        {
                            if (oldMember.Name != newPlayer.Name)
                            {
                                db.Add(new PlayerEvent
                                {
                                    ClanTag = newPlayer.ClanTag,
                                    PlayerTag = newPlayer.Tag,
                                    EventDate = DateTime.UtcNow,
                                    EventType = PlayerEventType.NameChange,
                                    StringValue = oldMember.Name
                                });
                                oldMember.Name = newPlayer.Name;
                            }
                        }
                    }

                    oldPlayer.AttackWins = newPlayer.AttackWins;
                    oldPlayer.BestTrophies = newPlayer.BestTrophies;
                    oldPlayer.DefenseWins = newPlayer.DefenseWins;
                    oldPlayer.Name = newPlayer.Name;
                    oldPlayer.TownHallLevel = newPlayer.TownHallLevel;
                    oldPlayer.WarStars = newPlayer.WarStars;
                    oldPlayer.LastUpdated = DateTime.UtcNow;

                    db.SaveChanges();
                }

                status.Message = playerName;
                status.Status = true;
            }
            catch (Exception e)
            {
                status.Message = string.Format("{0} Failed: {1}", playerName, e.Message);
                status.Status = false;
            }

            return status;
        }
    }
}
