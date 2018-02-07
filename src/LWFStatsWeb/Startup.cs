using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using LWFStatsWeb.Services;
using LWFStatsWeb.Logic;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using NLog.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Formatters;
using LWFStatsWeb.Formatters;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;

namespace LWFStatsWeb
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }


        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options => {
                //options.EnableSensitiveDataLogging(true);
                var connectionType = Configuration.GetConnectionString("Default");
                var connectionString = Configuration.GetConnectionString(connectionType);
                if (connectionType.Equals("SQLite"))
                    options.UseSqlite(connectionString);
                //else if (connectionType.Equals("MySQL"))
                //    options.UseMySql(connectionString);
                else if (connectionType.Equals("SqlServer"))
                    options.UseSqlServer(connectionString);
                else
                    throw new Exception($"Invalid connection type ${connectionType}");
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddOptions();
            services.Configure<ClanListOptions>(Configuration.GetSection("ClanLists"));
            services.Configure<ClashApiOptions>(Configuration.GetSection("ClashApi"));
            services.Configure<StatisicsOptions>(Configuration.GetSection("Statistics"));
            services.Configure<WeightSubmitOptions>(Configuration.GetSection("WeightSubmit"));
            services.Configure<WeightDatabaseOptions>(Configuration.GetSection("WeightDatabase"));
            services.Configure<WeightResultOptions>(Configuration.GetSection("ResultDatabase"));
            services.Configure<GoogleServiceOptions>(Configuration.GetSection("GoogleService"));
            services.Configure<GlobalOptions>(Configuration.GetSection("Options"));

            //services.Configure<GoogleOptions>(Configuration.GetSection("GoogleAuth"));

            var csvFormatterOptions = new CsvFormatterOptions();

            services.AddMvc(options =>
            {
                options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                options.InputFormatters.Add(new CsvInputFormatter(csvFormatterOptions));
                options.OutputFormatters.Add(new CsvOutputFormatter(csvFormatterOptions));

                options.FormatterMappings.SetMediaTypeMappingForFormat("csv", MediaTypeHeaderValue.Parse("text/csv"));
                options.FormatterMappings.SetMediaTypeMappingForFormat("xml", new MediaTypeHeaderValue("application/xml"));
            });

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
            services.AddTransient<IGoogleSheetsService, GoogleSheetsService>();
            services.AddSingleton<WeightSubmitService>();
            services.AddSingleton<IHostedService, HostedWebSubmitService>();

            // Caching
            services.AddMemoryCache();

            //services.AddLogging();

            //services.AddAuthorization(options =>
            //{
            //    options.AddPolicy("AdministratorOnly", policy => policy.RequireRole("Administrator"));
            //    options.AddPolicy("EmployeeId", policy => policy.RequireClaim("EmployeeId", "123", "456"));
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else if(env.IsStaging())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            // Add external authentication middleware below. To configure them please see http://go.microsoft.com/fwlink/?LinkID=532715

            //app.UseGoogleAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
