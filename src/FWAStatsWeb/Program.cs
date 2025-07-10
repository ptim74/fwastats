using FWAStatsWeb.Data;
using FWAStatsWeb.Formatters;
using FWAStatsWeb.Logic;
using FWAStatsWeb.Models;
using FWAStatsWeb.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using NLog.Extensions.Logging;
using System;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionType = builder.Configuration.GetConnectionString("Default");
    var connectionString = builder.Configuration.GetConnectionString(connectionType);
    if (connectionType.Equals("SQLite"))
        options.UseSqlite(connectionString);
    else if (connectionType.Equals("SqlServer"))
        options.UseSqlServer(connectionString);
    else
        throw new Exception($"Invalid connection type {connectionType}");
});

// Logging configuration
builder.Logging.ClearProviders();
builder.Logging.AddNLog();

// Identity configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Google authentication
var googleAuthEnabled = builder.Configuration.GetValue<bool>("Authentication:Google:Enabled");
if (googleAuthEnabled)
{
    builder.Services.AddAuthentication().AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    });
}

// Cookie configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});

// Options configuration
builder.Services.Configure<ClanListOptions>(builder.Configuration.GetSection("ClanLists"));
builder.Services.Configure<ClashApiOptions>(builder.Configuration.GetSection("ClashApi"));
builder.Services.Configure<StatisticsOptions>(builder.Configuration.GetSection("Statistics"));
builder.Services.Configure<WeightSubmitOptions>(builder.Configuration.GetSection("WeightSubmit"));
builder.Services.Configure<WeightDatabaseOptions>(builder.Configuration.GetSection("WeightDatabase"));
builder.Services.Configure<WeightResultOptions>(builder.Configuration.GetSection("ResultDatabase"));
builder.Services.Configure<GoogleServiceOptions>(builder.Configuration.GetSection("GoogleService"));
builder.Services.Configure<SendGridOptions>(builder.Configuration.GetSection("SendGrid"));

// MVC and formatters configuration
var csvFormatterOptions = new CsvFormatterOptions();

builder.Services.AddControllers(options =>
{
    options.OutputFormatters.Add(new XmlSerializerOutputFormatter());
    options.InputFormatters.Add(new CsvInputFormatter(csvFormatterOptions));
    options.OutputFormatters.Add(new CsvOutputFormatter(csvFormatterOptions));

    options.FormatterMappings.SetMediaTypeMappingForFormat("csv", MediaTypeHeaderValue.Parse("text/csv"));
    options.FormatterMappings.SetMediaTypeMappingForFormat("xml", new MediaTypeHeaderValue("application/xml"));
});

builder.Services.AddRazorPages();

// Application services
builder.Services.AddTransient<ISmsSender, AuthMessageSender>();

// Clash related services
builder.Services.AddTransient<IClanLoader, ClanLoader>();
builder.Services.AddTransient<IClanUpdater, ClanUpdater>();
builder.Services.AddTransient<IMemberUpdater, MemberUpdater>();
builder.Services.AddTransient<IClanStatistics, ClanStatistics>();
builder.Services.AddTransient<IClashApi, ClashApi>();
builder.Services.AddTransient<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddTransient<IGoogleSheetsService, GoogleSheetsService>();
builder.Services.AddSingleton<WeightSubmitService>();
builder.Services.AddSingleton<IHostedService, HostedWebSubmitService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();

// Data protection
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(builder.Configuration.GetValue<string>("KeyStorageFolder")))
    .SetApplicationName("FwaStats");

// Caching and HTTP client
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

// WebOptimizer for bundling and minification
builder.Services.AddWebOptimizer(pipeline =>
{
    // Bundle and minify CSS files
    pipeline.AddCssBundle("/css/site.min.css", "css/site.css");

    // Bundle and minify JavaScript files
    pipeline.AddJavaScriptBundle("/js/site.min.js", "js/site.js");
});

// Forwarded headers
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
}

app.UseForwardedHeaders();
app.UseWebOptimizer();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
