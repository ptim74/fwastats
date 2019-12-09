using LWFStatsWeb.Data;
using LWFStatsWeb.Models;
using LWFStatsWeb.Models.TrackerViewModels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public interface ITrackerUtility
    {
        Task<ICollection<Donation>> GetDonations(string id);
        string AuthorizeUrl(string state);
        Task<CallbackViewModel> GetAccessToken(string code);
    }

    public class TrackerUtility : ITrackerUtility
    {
        private readonly ApplicationDbContext db;
        private readonly IOptions<TrackerOptions> options;
        private readonly ILogger<ClanStatistics> logger;
        private readonly IClashApi api;
        private readonly IHttpClientFactory clientFactory;

        public TrackerUtility(
            ApplicationDbContext db,
            IOptions<TrackerOptions> options,
            ILogger<ClanStatistics> logger,
            IClashApi api,
            IHttpClientFactory clientFactory
        )
        {
            this.db = db;
            this.options = options;
            this.logger = logger;
            this.api = api;
            this.clientFactory = clientFactory;
        }

        public async Task<ICollection<Donation>> GetDonations(string id)
        {
            var tag = Utils.LinkIdToTag(id);

            var data = new List<Donation>();

            var clan = await api.GetClan(tag, false, false);

            if (clan.MemberList != null)
            {
                foreach (var member in clan.MemberList)
                {
                    data.Add(new Donation
                    {
                        Tag = member.Tag,
                        Name = member.Name,
                        Donated = member.Donations,
                        Received = member.DonationsReceived
                    });
                }
            }

            return data;
        }

        public string AuthorizeUrl(string state)
        {
            return string.Format("https://discordapp.com/api/oauth2/authorize?client_id={0}&redirect_uri={1}&response_type={2}&scope={3}&state={4}",
                Uri.EscapeDataString(options.Value.ClientId), 
                Uri.EscapeDataString(options.Value.CallbackUri), 
                "code",
                "webhook.incoming",
                Uri.EscapeDataString(state));
        }

        public async Task<CallbackViewModel> GetAccessToken(string code)
        {
            var client = clientFactory.CreateClient();

            var input = string.Format("client_id={0}&client_secret={1}&grant_type={2}&code={3}&redirect_uri={4}&scope={5}", 
                Uri.EscapeDataString(options.Value.ClientId),
                Uri.EscapeDataString(options.Value.ClientSecret),
                "authorization_code",
                Uri.EscapeDataString(code),
                Uri.EscapeDataString(options.Value.CallbackUri),
                "webhook.incoming");

            var content = new StringContent(input, Encoding.UTF8, "application/x-www-form-urlencoded");
            var result = await client.PostAsync("https://discordapp.com/api/oauth2/token", content);

            var response = await result.Content.ReadAsStringAsync();

            var token = JsonConvert.DeserializeObject<CallbackViewModel>(response);

            return token;
        }
    }
}
