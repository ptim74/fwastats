using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models;

namespace LWFStatsWeb.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20170429175041_WarOpponent")]
    partial class WarOpponent
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("LWFStatsWeb.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<string>("UserName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.Clan", b =>
                {
                    b.Property<string>("Tag")
                        .HasMaxLength(10);

                    b.Property<string>("BadgeUrl")
                        .HasMaxLength(150);

                    b.Property<int>("ClanLevel");

                    b.Property<int>("ClanPoints");

                    b.Property<string>("Description")
                        .HasMaxLength(300);

                    b.Property<int>("EstimatedWeight");

                    b.Property<string>("Group")
                        .HasMaxLength(10);

                    b.Property<bool>("IsWarLogPublic");

                    b.Property<string>("LocationName")
                        .HasMaxLength(30);

                    b.Property<double>("MatchPercentage");

                    b.Property<int>("Members");

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<int>("RequiredTrophies");

                    b.Property<int>("Th10Count");

                    b.Property<int>("Th11Count");

                    b.Property<int>("Th8Count");

                    b.Property<int>("Th9Count");

                    b.Property<int>("ThLowCount");

                    b.Property<string>("Type")
                        .HasMaxLength(10);

                    b.Property<int>("WarCount");

                    b.Property<string>("WarFrequency")
                        .HasMaxLength(20);

                    b.Property<int>("WarLosses");

                    b.Property<int>("WarTies");

                    b.Property<int>("WarWinStreak");

                    b.Property<int>("WarWins");

                    b.Property<double>("WinPercentage");

                    b.HasKey("Tag");

                    b.HasIndex("Name");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.ClanEvent", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Activity");

                    b.Property<string>("ClanTag")
                        .HasMaxLength(10);

                    b.Property<int>("Donations");

                    b.Property<DateTime>("EventDate");

                    b.HasKey("ID");

                    b.HasIndex("EventDate");

                    b.HasIndex("ClanTag", "EventDate");

                    b.ToTable("ClanEvents");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.ClanValidity", b =>
                {
                    b.Property<string>("Tag")
                        .HasMaxLength(10);

                    b.Property<string>("Group")
                        .HasMaxLength(10);

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<DateTime>("ValidFrom");

                    b.Property<DateTime>("ValidTo");

                    b.HasKey("Tag");

                    b.HasIndex("ValidFrom");

                    b.HasIndex("ValidTo");

                    b.ToTable("ClanValidities");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.Member", b =>
                {
                    b.Property<string>("Tag")
                        .HasMaxLength(10);

                    b.Property<string>("BadgeUrl")
                        .HasMaxLength(150);

                    b.Property<int>("ClanRank");

                    b.Property<string>("ClanTag")
                        .HasMaxLength(10);

                    b.Property<int>("Donations");

                    b.Property<int>("DonationsReceived");

                    b.Property<int>("ExpLevel");

                    b.Property<string>("LeagueName")
                        .HasMaxLength(30);

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<string>("Role")
                        .HasMaxLength(10);

                    b.Property<int>("Trophies");

                    b.HasKey("Tag");

                    b.HasIndex("ClanTag");

                    b.HasIndex("Name");

                    b.HasIndex("Role");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.Player", b =>
                {
                    b.Property<string>("Tag")
                        .HasMaxLength(10);

                    b.Property<int>("AttackWins");

                    b.Property<int>("BestTrophies");

                    b.Property<int>("DefenseWins");

                    b.Property<DateTime>("LastUpdated");

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<int>("TownHallLevel");

                    b.Property<int>("WarStars");

                    b.HasKey("Tag");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.PlayerEvent", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClanTag")
                        .HasMaxLength(10);

                    b.Property<DateTime>("EventDate");

                    b.Property<int>("EventType");

                    b.Property<string>("PlayerTag")
                        .HasMaxLength(10);

                    b.Property<int>("Value");

                    b.HasKey("ID");

                    b.HasIndex("ClanTag", "EventDate");

                    b.HasIndex("EventType", "EventDate");

                    b.HasIndex("PlayerTag", "EventDate");

                    b.ToTable("PlayerEvents");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.UpdateTask", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClanGroup")
                        .HasMaxLength(10);

                    b.Property<string>("ClanName")
                        .HasMaxLength(50);

                    b.Property<string>("ClanTag")
                        .HasMaxLength(10);

                    b.Property<int>("Mode");

                    b.HasKey("ID");

                    b.ToTable("UpdateTasks");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.War", b =>
                {
                    b.Property<string>("ID")
                        .HasMaxLength(30);

                    b.Property<int>("ClanAttacks");

                    b.Property<string>("ClanBadgeUrl")
                        .HasMaxLength(150);

                    b.Property<double>("ClanDestructionPercentage");

                    b.Property<int>("ClanExpEarned");

                    b.Property<int>("ClanLevel");

                    b.Property<string>("ClanName")
                        .HasMaxLength(50);

                    b.Property<int>("ClanStars");

                    b.Property<string>("ClanTag")
                        .HasMaxLength(10);

                    b.Property<DateTime>("EndTime");

                    b.Property<bool>("Friendly");

                    b.Property<bool>("Matched");

                    b.Property<string>("OpponentBadgeUrl")
                        .HasMaxLength(150);

                    b.Property<double>("OpponentDestructionPercentage");

                    b.Property<int>("OpponentLevel");

                    b.Property<string>("OpponentName")
                        .HasMaxLength(50);

                    b.Property<int>("OpponentStars");

                    b.Property<string>("OpponentTag")
                        .HasMaxLength(10);

                    b.Property<string>("Result")
                        .HasMaxLength(10);

                    b.Property<bool>("Synced");

                    b.Property<int>("TeamSize");

                    b.HasKey("ID");

                    b.HasIndex("ClanTag");

                    b.HasIndex("EndTime");

                    b.HasIndex("OpponentTag", "EndTime");

                    b.ToTable("Wars");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarAttack", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("AttackerTag")
                        .HasMaxLength(10);

                    b.Property<int>("DefenderMapPosition");

                    b.Property<string>("DefenderTag")
                        .HasMaxLength(10);

                    b.Property<int>("DefenderTownHallLevel");

                    b.Property<int>("DestructionPercentage");

                    b.Property<bool>("IsOpponent");

                    b.Property<int>("Order");

                    b.Property<int>("Stars");

                    b.Property<string>("WarID")
                        .HasMaxLength(30);

                    b.HasKey("ID");

                    b.HasIndex("AttackerTag");

                    b.HasIndex("DefenderTag");

                    b.HasIndex("WarID", "Order");

                    b.ToTable("WarAttacks");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarMember", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsOpponent");

                    b.Property<int>("MapPosition");

                    b.Property<string>("Name")
                        .HasMaxLength(50);

                    b.Property<int>("OpponentAttacks");

                    b.Property<string>("Tag")
                        .HasMaxLength(10);

                    b.Property<int>("TownHallLevel");

                    b.Property<string>("WarID")
                        .HasMaxLength(30);

                    b.HasKey("ID");

                    b.HasIndex("Tag");

                    b.HasIndex("WarID", "MapPosition");

                    b.ToTable("WarMembers");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarSync", b =>
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

            modelBuilder.Entity("LWFStatsWeb.Models.Weight", b =>
                {
                    b.Property<string>("Tag")
                        .HasMaxLength(10);

                    b.Property<bool>("InWar");

                    b.Property<DateTime>("LastModified");

                    b.Property<int>("WarWeight");

                    b.HasKey("Tag");

                    b.ToTable("Weights");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

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

            modelBuilder.Entity("LWFStatsWeb.Models.Member", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.Clan", "Clan")
                        .WithMany("MemberList")
                        .HasForeignKey("ClanTag");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarAttack", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.War")
                        .WithMany("Attacks")
                        .HasForeignKey("WarID");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarMember", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.War")
                        .WithMany("Members")
                        .HasForeignKey("WarID");
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
                    b.HasOne("LWFStatsWeb.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.ApplicationUser")
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

                    b.HasOne("LWFStatsWeb.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
        }
    }
}
