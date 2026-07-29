using FWAStatsWeb.Data;
using FWAStatsWeb.Formatters;
using FWAStatsWeb.Logic;
using FWAStatsWeb.Models;
using FWAStatsWeb.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NLog.Extensions.Logging;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionType = builder.Configuration.GetConnectionString("Default");
    if (string.IsNullOrWhiteSpace(connectionType))
        throw new InvalidOperationException("ConnectionStrings:Default is not configured.");

    var connectionString = builder.Configuration.GetConnectionString(connectionType);
    if (connectionType.Equals("SQLite"))
        options.UseSqlite(connectionString);
    else if (connectionType.Equals("SqlServer"))
        options.UseSqlServer(connectionString);
    else
        throw new InvalidOperationException($"Invalid connection type {connectionType}");
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
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));

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
builder.Services.AddHttpClient<IClashApi, ClashApi>((serviceProvider, client) =>
{
    var clashApiOptions = serviceProvider.GetRequiredService<IOptions<ClashApiOptions>>().Value;
    // Fully qualified: System.Net.Http.Headers cannot be imported here without making
    // MediaTypeHeaderValue ambiguous against Microsoft.Net.Http.Headers below.
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", clashApiOptions.Token);
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    // Replaces the hand-rolled gzip handling ClashApi used to do, and sends the
    // matching Accept-Encoding request header for us.
    AutomaticDecompression = DecompressionMethods.All
});
builder.Services.AddTransient<IGoogleCalendarService, GoogleCalendarService>();
builder.Services.AddTransient<IGoogleSheetsService, GoogleSheetsService>();
builder.Services.AddSingleton<WeightSubmitService>();
builder.Services.AddHostedService<HostedWebSubmitService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

// Data protection
// .NET 10 preserves null values from configuration instead of dropping the key, so this
// can now come back null and must not be handed straight to DirectoryInfo.
var keyStorageFolder = builder.Configuration.GetValue<string>("KeyStorageFolder");
if (string.IsNullOrWhiteSpace(keyStorageFolder))
    throw new InvalidOperationException("KeyStorageFolder is not configured.");

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keyStorageFolder))
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
