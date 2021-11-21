using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using FWAStatsWeb.Data;

namespace FWAStatsWeb.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20161019183921_Players")]
    partial class Players
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.1");

            modelBuilder.Entity("FWAStatsWeb.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasDatabaseName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasDatabaseName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Clan", b =>
                {
                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("BadgeUrl")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<int>("ClanLevel");

                    b.Property<int>("ClanPoints");

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 300);

                    b.Property<string>("Group")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<bool>("IsWarLogPublic");

                    b.Property<string>("LocationName")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<int>("Members");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("RequiredTrophies");

                    b.Property<string>("Type")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("WarFrequency")
                        .HasAnnotation("MaxLength", 20);

                    b.Property<int>("WarLosses");

                    b.Property<int>("WarTies");

                    b.Property<int>("WarWinStreak");

                    b.Property<int>("WarWins");

                    b.HasKey("Tag");

                    b.HasIndex("Name");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.ClanValidity", b =>
                {
                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("Group")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<DateTime>("ValidFrom");

                    b.Property<DateTime>("ValidTo");

                    b.HasKey("Tag");

                    b.HasIndex("ValidFrom");

                    b.HasIndex("ValidTo");

                    b.ToTable("ClanValidities");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Member", b =>
                {
                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("BadgeUrl")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<int>("ClanRank");

                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("Donations");

                    b.Property<int>("DonationsReceived");

                    b.Property<int>("ExpLevel");

                    b.Property<string>("LeagueName")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("Role")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("Trophies");

                    b.HasKey("Tag");

                    b.HasIndex("ClanTag");

                    b.HasIndex("Name");

                    b.HasIndex("Role");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Player", b =>
                {
                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("AttackWins");

                    b.Property<int>("BestTrophies");

                    b.Property<int>("DefenseWins");

                    b.Property<DateTime>("LastUpdated");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("TownHallLevel");

                    b.Property<int>("WarStars");

                    b.HasKey("Tag");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.PlayerEvent", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<DateTime>("EventDate");

                    b.Property<int>("EventType");

                    b.Property<string>("PlayerTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("Value");

                    b.HasKey("ID");

                    b.HasIndex("ClanTag", "EventDate");

                    b.HasIndex("PlayerTag", "EventDate");

                    b.ToTable("PlayerEvents");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.UpdateTask", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClanGroup")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("ClanName")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("Mode");

                    b.HasKey("ID");

                    b.ToTable("UpdateTasks");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.War", b =>
                {
                    b.Property<string>("ID")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<int>("ClanAttacks");

                    b.Property<string>("ClanBadgeUrl")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<double>("ClanDestructionPercentage");

                    b.Property<int>("ClanExpEarned");

                    b.Property<int>("ClanLevel");

                    b.Property<string>("ClanName")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("ClanStars");

                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<DateTime>("EndTime");

                    b.Property<bool>("Matched");

                    b.Property<string>("OpponentBadgeUrl")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<double>("OpponentDestructionPercentage");

                    b.Property<int>("OpponentLevel");

                    b.Property<string>("OpponentName")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("OpponentStars");

                    b.Property<string>("OpponentTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("Result")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<bool>("Synced");

                    b.Property<int>("TeamSize");

                    b.HasKey("ID");

                    b.HasIndex("ClanTag");

                    b.HasIndex("EndTime");

                    b.HasIndex("OpponentTag", "EndTime");

                    b.ToTable("Wars");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.WarSync", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AllianceMatches");

                    b.Property<DateTime>("Finish");

                    b.Property<int>("MissedStarts");

                    b.Property<DateTime>("Start");

                    b.Property<int>("WarMatches");

                    b.HasKey("ID");

                    b.HasIndex("Finish");

                    b.HasIndex("Start");

                    b.ToTable("WarSyncs");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Weight", b =>
                {
                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<bool>("InWar");

                    b.Property<DateTime>("LastModified");

                    b.Property<int>("WarWeight");

                    b.HasKey("Tag");

                    b.ToTable("Weights");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasDatabaseName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Member", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.Clan", "Clan")
                        .WithMany("MemberList")
                        .HasForeignKey("ClanTag");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("FWAStatsWeb.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
