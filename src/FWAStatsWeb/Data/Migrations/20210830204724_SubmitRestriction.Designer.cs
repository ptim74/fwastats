﻿// <auto-generated />
using System;
using FWAStatsWeb.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FWAStatsWeb.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20210830204724_SubmitRestriction")]
    partial class SubmitRestriction
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.15");

            modelBuilder.Entity("FWAStatsWeb.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<int>("AccessFailedCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<bool>("EmailConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("LockoutEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<DateTimeOffset?>("LockoutEnd")
                        .HasColumnType("TEXT");

                    b.Property<string>("NormalizedEmail")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedUserName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<string>("PasswordHash")
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("TEXT");

                    b.Property<bool>("PhoneNumberConfirmed")
                        .HasColumnType("INTEGER");

                    b.Property<string>("SecurityStamp")
                        .HasColumnType("TEXT");

                    b.Property<bool>("TwoFactorEnabled")
                        .HasColumnType("INTEGER");

                    b.Property<string>("UserName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.BlacklistedClan", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.HasKey("Tag");

                    b.ToTable("BlacklistedClans");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Clan", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<string>("BadgeUrl")
                        .HasColumnType("TEXT")
                        .HasMaxLength(150);

                    b.Property<int>("ClanLevel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClanPoints")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Description")
                        .HasColumnType("TEXT")
                        .HasMaxLength(300);

                    b.Property<int>("EstimatedWeight")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Group")
                        .HasColumnType("TEXT")
                        .HasMaxLength(10);

                    b.Property<bool>("InLeague")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsWarLogPublic")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LocationName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(30);

                    b.Property<double>("MatchPercentage")
                        .HasColumnType("REAL");

                    b.Property<int>("Members")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<int>("RequiredTrophies")
                        .HasColumnType("INTEGER");

                    b.Property<int>("SubmitRestriction")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Th10Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Th11Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Th12Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Th13Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Th14Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Th8Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Th9Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ThLowCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Type")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("WarCount")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WarFrequency")
                        .HasColumnType("TEXT")
                        .HasMaxLength(20);

                    b.Property<int>("WarLosses")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarTies")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarWinStreak")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarWins")
                        .HasColumnType("INTEGER");

                    b.Property<double>("WinPercentage")
                        .HasColumnType("REAL");

                    b.HasKey("Tag");

                    b.HasIndex("Name");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.ClanEvent", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Activity")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("Donations")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("TEXT");

                    b.HasKey("ID");

                    b.HasIndex("EventDate");

                    b.HasIndex("ClanTag", "EventDate");

                    b.ToTable("ClanEvents");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.ClanValidity", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<string>("Group")
                        .HasColumnType("TEXT")
                        .HasMaxLength(10);

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<DateTime>("ValidFrom")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("ValidTo")
                        .HasColumnType("TEXT");

                    b.HasKey("Tag");

                    b.HasIndex("ValidFrom");

                    b.HasIndex("ValidTo");

                    b.ToTable("ClanValidities");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Member", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<string>("BadgeUrl")
                        .HasColumnType("TEXT")
                        .HasMaxLength(150);

                    b.Property<int>("ClanRank")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("Donations")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DonationsReceived")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ExpLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LeagueName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(30);

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<string>("Role")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("Trophies")
                        .HasColumnType("INTEGER");

                    b.HasKey("Tag");

                    b.HasIndex("ClanTag");

                    b.HasIndex("Name");

                    b.HasIndex("Role");

                    b.ToTable("Members");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Player", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("AttackWins")
                        .HasColumnType("INTEGER");

                    b.Property<int>("BestTrophies")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DefenseWins")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastUpdated")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<int>("TownHallLevel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarStars")
                        .HasColumnType("INTEGER");

                    b.HasKey("Tag");

                    b.ToTable("Players");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.PlayerClaim", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<string>("UserId")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.HasKey("Tag");

                    b.HasIndex("UserId");

                    b.ToTable("PlayerClaims");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.PlayerEvent", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<DateTime>("EventDate")
                        .HasColumnType("TEXT");

                    b.Property<int>("EventType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("PlayerTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<string>("StringValue")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("Value")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ClanTag", "EventDate");

                    b.HasIndex("EventType", "EventDate");

                    b.HasIndex("PlayerTag", "EventDate");

                    b.ToTable("PlayerEvents");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.UpdateTask", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("ClanGroup")
                        .HasColumnType("TEXT")
                        .HasMaxLength(10);

                    b.Property<string>("ClanName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<string>("ClanTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("Mode")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.ToTable("UpdateTasks");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.UserDetail", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<DateTime>("Created")
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("LastLogin")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("UserDetails");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.War", b =>
                {
                    b.Property<string>("ID")
                        .HasColumnType("TEXT")
                        .HasMaxLength(30);

                    b.Property<int>("ClanAttacks")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanBadgeUrl")
                        .HasColumnType("TEXT")
                        .HasMaxLength(150);

                    b.Property<double>("ClanDestructionPercentage")
                        .HasColumnType("REAL");

                    b.Property<int>("ClanExpEarned")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClanLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<int>("ClanStars")
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClanTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<DateTime>("EndTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Friendly")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("Matched")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OpponentBadgeUrl")
                        .HasColumnType("TEXT")
                        .HasMaxLength(150);

                    b.Property<double>("OpponentDestructionPercentage")
                        .HasColumnType("REAL");

                    b.Property<int>("OpponentLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OpponentName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<int>("OpponentStars")
                        .HasColumnType("INTEGER");

                    b.Property<string>("OpponentTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<DateTime>("PreparationStartTime")
                        .HasColumnType("TEXT");

                    b.Property<string>("Result")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Synced")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TeamSize")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("ClanTag");

                    b.HasIndex("EndTime");

                    b.HasIndex("PreparationStartTime");

                    b.HasIndex("OpponentTag", "EndTime");

                    b.ToTable("Wars");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.WarAttack", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("AttackerTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("DefenderMapPosition")
                        .HasColumnType("INTEGER");

                    b.Property<string>("DefenderTag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("DefenderTownHallLevel")
                        .HasColumnType("INTEGER");

                    b.Property<int>("DestructionPercentage")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsOpponent")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Order")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Stars")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WarID")
                        .HasColumnType("TEXT")
                        .HasMaxLength(30);

                    b.HasKey("ID");

                    b.HasIndex("AttackerTag");

                    b.HasIndex("DefenderTag");

                    b.HasIndex("WarID", "Order");

                    b.ToTable("WarAttacks");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.WarMember", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsOpponent")
                        .HasColumnType("INTEGER");

                    b.Property<int>("MapPosition")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(50);

                    b.Property<int>("OpponentAttacks")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("TownHallLevel")
                        .HasColumnType("INTEGER");

                    b.Property<string>("WarID")
                        .HasColumnType("TEXT")
                        .HasMaxLength(30);

                    b.HasKey("ID");

                    b.HasIndex("Tag");

                    b.HasIndex("WarID", "MapPosition");

                    b.ToTable("WarMembers");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.WarSync", b =>
                {
                    b.Property<int>("ID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("AllianceMatches")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Finish")
                        .HasColumnType("TEXT");

                    b.Property<int>("MissedStarts")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Start")
                        .HasColumnType("TEXT");

                    b.Property<bool>("Verified")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarMatches")
                        .HasColumnType("INTEGER");

                    b.HasKey("ID");

                    b.HasIndex("Finish");

                    b.HasIndex("Start");

                    b.ToTable("WarSyncs");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Weight", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("ExtWeight")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("InWar")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("LastModified")
                        .HasColumnType("TEXT");

                    b.Property<int>("SyncWeight")
                        .HasColumnType("INTEGER");

                    b.Property<int>("WarWeight")
                        .HasColumnType("INTEGER");

                    b.HasKey("Tag");

                    b.ToTable("Weights");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.WeightResult", b =>
                {
                    b.Property<string>("Tag")
                        .HasColumnType("TEXT")
                        .HasMaxLength(15);

                    b.Property<int>("Base01")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base02")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base03")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base04")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base05")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base06")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base07")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base08")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base09")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base10")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base11")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base12")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base13")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base14")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base15")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base16")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base17")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base18")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base19")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base20")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base21")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base22")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base23")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base24")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base25")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base26")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base27")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base28")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base29")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base30")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base31")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base32")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base33")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base34")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base35")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base36")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base37")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base38")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base39")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base40")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base41")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base42")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base43")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base44")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base45")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base46")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base47")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base48")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base49")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Base50")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("PendingResult")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH10Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH11Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH12Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH13Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH14Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH7Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH8Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TH9Count")
                        .HasColumnType("INTEGER");

                    b.Property<int>("THSum")
                        .HasColumnType("INTEGER");

                    b.Property<int>("TeamSize")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("TEXT");

                    b.Property<int>("Weight")
                        .HasColumnType("INTEGER");

                    b.HasKey("Tag");

                    b.ToTable("WeightResults");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRole", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("TEXT");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.Property<string>("NormalizedName")
                        .HasColumnType("TEXT")
                        .HasMaxLength(256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .IsUnique()
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("ClaimType")
                        .HasColumnType("TEXT");

                    b.Property<string>("ClaimValue")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderKey")
                        .HasColumnType("TEXT");

                    b.Property<string>("ProviderDisplayName")
                        .HasColumnType("TEXT");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("RoleId")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId")
                        .HasColumnType("TEXT");

                    b.Property<string>("LoginProvider")
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("Value")
                        .HasColumnType("TEXT");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.Member", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.Clan", "Clan")
                        .WithMany("MemberList")
                        .HasForeignKey("ClanTag");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.WarAttack", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.War", null)
                        .WithMany("Attacks")
                        .HasForeignKey("WarID");
                });

            modelBuilder.Entity("FWAStatsWeb.Models.WarMember", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.War", null)
                        .WithMany("Members")
                        .HasForeignKey("WarID");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.IdentityRole", null)
                        .WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("FWAStatsWeb.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.IdentityUserToken<string>", b =>
                {
                    b.HasOne("FWAStatsWeb.Models.ApplicationUser", null)
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
