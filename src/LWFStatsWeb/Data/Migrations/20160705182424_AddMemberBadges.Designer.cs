using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using LWFStatsWeb.Data;

namespace LWFStatsWeb.Data.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20160705182424_AddMemberBadges")]
    partial class AddMemberBadges
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("LWFStatsWeb.Models.ApplicationUser", b =>
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
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.Clan", b =>
                {
                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("ClanLevel");

                    b.Property<int>("ClanPoints");

                    b.Property<string>("ClanType")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("Description")
                        .HasAnnotation("MaxLength", 300);

                    b.Property<bool>("IsWarLogPublic");

                    b.Property<int>("MemberCount");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("RequiredTrophies");

                    b.Property<int>("WarLosses");

                    b.Property<int>("WarTies");

                    b.Property<int>("WarWinStreak");

                    b.Property<int>("WarWins");

                    b.HasKey("Tag");

                    b.HasIndex("Name");

                    b.ToTable("Clans");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.ClanBadgeUrls", b =>
                {
                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("Large")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<string>("Medium")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<string>("Small")
                        .HasAnnotation("MaxLength", 150);

                    b.HasKey("ClanTag");

                    b.HasIndex("ClanTag")
                        .IsUnique();

                    b.ToTable("ClanBadgeUrls");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.Member", b =>
                {
                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("ClanRank");

                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("Donations");

                    b.Property<int>("DonationsReceived");

                    b.Property<int>("ExpLevel");

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

            modelBuilder.Entity("LWFStatsWeb.Models.MemberBadgeUrls", b =>
                {
                    b.Property<string>("MemberTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<string>("Large")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<string>("Medium")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<string>("Small")
                        .HasAnnotation("MaxLength", 150);

                    b.HasKey("MemberTag");

                    b.HasIndex("MemberTag")
                        .IsUnique();

                    b.ToTable("MemberBadgeUrls");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.UpdateTask", b =>
                {
                    b.Property<Guid>("ID")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClanName")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("Mode");

                    b.HasKey("ID");

                    b.ToTable("UpdateTasks");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.War", b =>
                {
                    b.Property<string>("ID")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<string>("ClanTag")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<DateTime>("EndTime");

                    b.Property<string>("Result")
                        .HasAnnotation("MaxLength", 10);

                    b.Property<int>("TeamSize");

                    b.HasKey("ID");

                    b.HasIndex("ClanTag");

                    b.HasIndex("EndTime");

                    b.ToTable("Wars");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarClanResult", b =>
                {
                    b.Property<string>("WarID")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<int>("Attacks");

                    b.Property<int>("ClanLevel");

                    b.Property<double>("DestructionPercentage");

                    b.Property<int>("ExpEarned");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("Stars");

                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.HasKey("WarID");

                    b.HasIndex("Tag");

                    b.HasIndex("WarID")
                        .IsUnique();

                    b.ToTable("WarParticipants");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarOpponentBadgeUrls", b =>
                {
                    b.Property<string>("WarID")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<string>("Large")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<string>("Medium")
                        .HasAnnotation("MaxLength", 150);

                    b.Property<string>("Small")
                        .HasAnnotation("MaxLength", 150);

                    b.HasKey("WarID");

                    b.HasIndex("WarID")
                        .IsUnique();

                    b.ToTable("WarOpponentBadgeUrls");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarOpponentResult", b =>
                {
                    b.Property<string>("WarID")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<int>("ClanLevel");

                    b.Property<double>("DestructionPercentage");

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("Stars");

                    b.Property<string>("Tag")
                        .HasAnnotation("MaxLength", 10);

                    b.HasKey("WarID");

                    b.HasIndex("Tag");

                    b.HasIndex("WarID")
                        .IsUnique();

                    b.ToTable("WarOpponents");
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

            modelBuilder.Entity("LWFStatsWeb.Models.ClanBadgeUrls", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.Clan", "Clan")
                        .WithOne("BadgeUrl")
                        .HasForeignKey("LWFStatsWeb.Models.ClanBadgeUrls", "ClanTag")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LWFStatsWeb.Models.Member", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.Clan", "Clan")
                        .WithMany("Members")
                        .HasForeignKey("ClanTag");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.MemberBadgeUrls", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.Member", "Member")
                        .WithOne("BadgeUrl")
                        .HasForeignKey("LWFStatsWeb.Models.MemberBadgeUrls", "MemberTag")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LWFStatsWeb.Models.War", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.Clan", "Clan")
                        .WithMany("Wars")
                        .HasForeignKey("ClanTag");
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarClanResult", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.War", "War")
                        .WithOne("ClanResult")
                        .HasForeignKey("LWFStatsWeb.Models.WarClanResult", "WarID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarOpponentBadgeUrls", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.WarOpponentResult", "WarOpponent")
                        .WithOne("BadgeUrl")
                        .HasForeignKey("LWFStatsWeb.Models.WarOpponentBadgeUrls", "WarID")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("LWFStatsWeb.Models.WarOpponentResult", b =>
                {
                    b.HasOne("LWFStatsWeb.Models.War", "War")
                        .WithOne("OpponentResult")
                        .HasForeignKey("LWFStatsWeb.Models.WarOpponentResult", "WarID")
                        .OnDelete(DeleteBehavior.Cascade);
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
