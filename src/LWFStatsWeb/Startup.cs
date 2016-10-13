﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using LWFStatsWeb.Services;
using LWFStatsWeb.Logic;
using Microsoft.AspNetCore.Mvc;

namespace LWFStatsWeb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddJsonFile(@"d:\home\data\appsettings.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //Server=(localdb)\\mssqllocaldb;Database=aspnet-LWFStatsWeb-38c3a7ca-3bf0-4276-9c1f-b74988336664;Trusted_Connection=True;MultipleActiveResultSets=true

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOptions();
            services.Configure<ClanListOptions>(Configuration.GetSection("ClanLists"));
            services.Configure<ClashApiOptions>(Configuration.GetSection("ClashApi"));

            //services.Configure<GoogleOptions>(Configuration.GetSection("GoogleAuth"));

            services.AddMvc();

            //services.AddMvc(options =>
            //{
            //    options.Filters.Add(new RequireHttpsAttribute());
            //});

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // Clash related services
            services.AddTransient<IClanLoader, ClanLoader>();
            services.AddTransient<IClanUpdater, ClanUpdater>();
            services.AddTransient<IClanStatistics, ClanStatistics>();
            services.AddTransient<IClashApi, ClashApi>();

            // Caching
            services.AddMemoryCache();

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
            //    options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseIdentity();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            //app.UseGoogleAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute("ClanDetails", "Clan/{id}", new { controller = "Clans", action = "Details" });
                routes.MapRoute("ClanEdit", "Clan/{id}/Edit", new { controller = "Clans", action = "Edit" });
                routes.MapRoute("ClanWeight", "Clan/{id}/Weight", new { controller = "Clans", action = "Weight" });

                routes.MapRoute("SyncDetails", "Sync/{id}", new { controller = "Syncs", action = "Details" });
                routes.MapRoute("FWASyncDetails", "Sync/FWA/{id}", new { controller = "Syncs", action = "FWADetails" });
                routes.MapRoute("FWALSyncDetails", "Sync/FWAL/{id}", new { controller = "Syncs", action = "FWALDetails" });

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

        }
    }
}
