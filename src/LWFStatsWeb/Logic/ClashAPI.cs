using LWFStatsWeb.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class ClashApiError
    {
        public string Reason { get; set; }
        public string Message { get; set; }
    }

    public class ClashApiOptions
    {
        public string Url { get; set; }
        public string Token { get; set; }
    }

    public interface IClashApi
    {
        Task<ICollection<War>> GetClanWarlog(string clanTag);
        Task<War> GetClanCurrentWar(string clanTag);
        Task<Clan> GetClan(string clanTag, bool withWarDetails, bool withCurrentWar);
        Task<Player> GetPlayer(string playerTag);
    }

    public class ClashApi : IClashApi
    {
        private readonly IOptions<ClashApiOptions> options;

        public ClashApi(IOptions<ClashApiOptions> options)
        {
            this.options = options;
        }

        private async Task<string> Request(string page)
        {
            var url = string.Format("{0}/{1}", options.Value.Url, page);
            var request = WebRequest.Create(url);
            request.Headers["Authorization"] = string.Format("Bearer {0}", options.Value.Token);
            try
            {
                var response = await request.GetResponseAsync();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var data = await reader.ReadToEndAsync();
                    return data;
                }
            }
            catch (WebException e)
            {
                using (var reader = new StreamReader(e.Response.GetResponseStream()))
                {
                    var data = await reader.ReadToEndAsync();
                    var error = JsonConvert.DeserializeObject<ClashApiError>(data);
                    if(error != null)
                    {
                        var msg = $"API Error {e.Status}, Reason: {error.Reason}, Message: {error.Message}";
                        throw new Exception(msg, e);
                    }
                }
                throw e;
            }
        }

        private async Task<T> Request<T>(string page)
        {
            var pageData = await Request(page);

            var settings = new JsonSerializerSettings {
                NullValueHandling = NullValueHandling.Ignore,
                DateFormatString = "yyyyMMddTHHmmss.fffK"
            };

            return JsonConvert.DeserializeObject<T>(pageData, settings);
        }

        public async Task<ICollection<War>> GetClanWarlog(string clanTag)
        {
            var url = string.Format("clans/{0}/warlog", Uri.EscapeDataString(clanTag));
            var data = await Request<Warlog>(url);
            if (data == null)
                return null;
            return data.Wars;
        }

        public async Task<War> GetClanCurrentWar(string clanTag)
        {
            var url = string.Format("clans/{0}/currentwar", Uri.EscapeDataString(clanTag));
            var data = await Request<War>(url);
            return data;
        }

        public async Task<Clan> GetClan(string clanTag, bool withWarDetails, bool withCurrentWar)
        {
            var url = string.Format("clans/{0}", Uri.EscapeDataString(clanTag));
            var data = await Request<Clan>(url);

            if (data.IsWarLogPublic && withWarDetails)
                data.Wars = await GetClanWarlog(clanTag);

            if(data.IsWarLogPublic && withCurrentWar)
            {
                var currentWar = await GetClanCurrentWar(clanTag);

                if (currentWar != null)
                {
                    if (data.Wars == null)
                        data.Wars = new List<War>();

                    if (!string.IsNullOrEmpty(currentWar.State) && currentWar.State.Equals("warEnded"))
                    {
                        var prevWar = data.Wars.SingleOrDefault( w => 
                            w.EndTime >= currentWar.EndTime.AddMinutes(-1) && 
                            w.EndTime <= currentWar.EndTime.AddMinutes(1) &&
                            w.OpponentTag == currentWar.OpponentTag &&
                            w.TeamSize == currentWar.TeamSize
                            );
                        if (prevWar != null)
                        {
                            currentWar.FixData(DateTime.MinValue);
                            prevWar.Members = currentWar.Members;
                            if (prevWar.Members != null)
                                foreach (var member in prevWar.Members)
                                    member.WarID = prevWar.ID;
                            prevWar.Attacks = currentWar.Attacks;
                            if (prevWar.Attacks != null)
                                foreach (var attack in prevWar.Attacks)
                                    attack.WarID = prevWar.ID;
                            prevWar.StartTime = currentWar.StartTime;
                            prevWar.PreparationStartTime = currentWar.PreparationStartTime;
                        }
                    }
                    else if (!string.IsNullOrEmpty(currentWar.State) && !currentWar.State.Equals("notInWar"))
                    {
                        data.Wars.Add(currentWar);
                    }
                }
            }

            data.FixData();

            return data;
        }

        public async Task<Player> GetPlayer(string playerTag)
        {
            var url = string.Format("players/{0}", Uri.EscapeDataString(playerTag));
            var data = await Request<Player>(url);

            data.FixData();

            return data;
        }
    }
}
