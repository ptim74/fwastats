using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using FWAStatsWeb.Models;

namespace FWAStatsWeb.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            builder.Entity<Clan>().HasIndex(e => e.Name);
            builder.Entity<Member>().HasIndex(e => e.Name);
            builder.Entity<Member>().HasIndex(e => e.Role);
            builder.Entity<War>().HasIndex(e => e.EndTime);
            builder.Entity<War>().HasIndex(e => e.ClanTag);
            builder.Entity<War>().HasIndex(e => new { e.OpponentTag, e.EndTime });
            builder.Entity<War>().HasIndex(e => e.PreparationStartTime);
            builder.Entity<WarSync>().HasIndex(e => e.Start);
            builder.Entity<WarSync>().HasIndex(e => e.Finish);
            builder.Entity<ClanValidity>().HasIndex(v => v.ValidFrom);
            builder.Entity<ClanValidity>().HasIndex(v => v.ValidTo);
            builder.Entity<PlayerClaim>().HasIndex(e => new { e.UserId });
            builder.Entity<PlayerEvent>().HasIndex(e => new { e.ClanTag, e.EventDate });
            builder.Entity<PlayerEvent>().HasIndex(e => new { e.PlayerTag, e.EventDate });
            builder.Entity<PlayerEvent>().HasIndex(e => new { e.EventType, e.EventDate });
            builder.Entity<ClanEvent>().HasIndex(e => new { e.ClanTag, e.EventDate });
            builder.Entity<ClanEvent>().HasIndex(e => new { e.EventDate });
            builder.Entity<WarMember>().HasIndex(e => new { e.WarID, e.MapPosition });
            builder.Entity<WarMember>().HasIndex(e => new { e.Tag });
            builder.Entity<WarAttack>().HasIndex(e => new { e.WarID, e.Order });
            builder.Entity<WarAttack>().HasIndex(e => new { e.AttackerTag });
            builder.Entity<WarAttack>().HasIndex(e => new { e.DefenderTag });
        }

        public virtual DbSet<Clan> Clans { get; set; }
        public virtual DbSet<Member> Members { get; set; }
        public virtual DbSet<Player> Players { get; set; }
        public virtual DbSet<PlayerClaim> PlayerClaims { get; set; }
        public virtual DbSet<PlayerEvent> PlayerEvents { get; set; }
        public virtual DbSet<UpdateTask> UpdateTasks { get; set; }
        public virtual DbSet<UserDetail> UserDetails { get; set; }
        public virtual DbSet<War> Wars { get; set; }
        public virtual DbSet<WarSync> WarSyncs { get; set; }
        public virtual DbSet<ClanValidity> ClanValidities { get; set; }
        public virtual DbSet<Weight> Weights { get; set; }
        public virtual DbSet<ClanEvent> ClanEvents { get; set; }
        public virtual DbSet<WarMember> WarMembers { get; set; }
        public virtual DbSet<WarAttack> WarAttacks { get; set; }
        public virtual DbSet<BlacklistedClan> BlacklistedClans { get; set; }
        public virtual DbSet<WeightResult> WeightResults { get; set; }

        // Add Migration:
        // CMD> dotnet ef migrations {MigrationName}
        // PM> Add-Migration [-Name] <String>
        // PM> Update-Database
    }
}
