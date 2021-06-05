using FWAStatsWeb.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace FWAStatsWeb.Logic
{
    public class ClashApiException : Exception
    {
        public ClashApiException(string message) : base(message)
        {
        }
        public ClashApiException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
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
        //Task<War> GetClanCurrentWar(string clanTag);
        Task<Clan> GetClan(string clanTag, bool withWarDetails, bool withCurrentWar);
        Task<Player> GetPlayer(string playerTag);
    }

    public class ClashApi : IClashApi
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IOptions<ClashApiOptions> options;

        public ClashApi(
            IOptions<ClashApiOptions> options,
            IHttpClientFactory clientFactory)
        {
            this.options = options;
            this.clientFactory = clientFactory;
        }

        private async Task<Stream> GetUncompressedResponseStream(HttpResponseMessage response)
        {
            Stream responseStream = await response.Content.ReadAsStreamAsync();
            if (response.Content.Headers.Contains("Content-Encoding"))
                foreach (var encoding in response.Content.Headers.GetValues("Content-Encoding"))
                    if ("gzip".Equals(encoding))
                        responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
            return responseStream;
        }

        private async Task<string> Request(string page)
        {
            var url = string.Format("{0}/{1}", options.Value.Url, page);
            var client = clientFactory.CreateClient();

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("Authorization", string.Format("Bearer {0}", options.Value.Token));
                    request.Headers.Add("Accept-Encoding", "gzip");

                    using (var response = await client.SendAsync(request))
                    {
                        using (var responseStream = await GetUncompressedResponseStream(response))
                        {
                            using (var reader = new StreamReader(responseStream))
                            {
                                var data = await reader.ReadToEndAsync();
                                if(!response.IsSuccessStatusCode)
                                {
                                    var error = JsonConvert.DeserializeObject<ClashApiError>(data);
                                    if (error != null)
                                    {
                                        var msg = $"API Error {response.ReasonPhrase}, Reason: {error.Reason}, Message: {error.Message}";
                                        throw new Exception(msg);
                                    }
                                    else
                                    {
                                        throw new Exception($"HTTP Error {response.ReasonPhrase}");
                                    }
                                }
                                return data;
                            }
                        }
                    }
                }
            } 
            catch(Exception e)
            {
                throw new ClashApiException(string.Format("Failed to get '{0}'", page), e);
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
            var url = string.Format("clans/{0}/warlog", Uri.EscapeDataString(clanTag.ToUpperInvariant()));
            var data = await Request<Warlog>(url);
            if (data == null || data.Wars == null)
                return null;

            var ret = new List<War>();
            foreach(var war in data.Wars)
            {
                war.FixData(DateTime.MinValue);
                if (!string.IsNullOrEmpty(war.Result) && !string.IsNullOrEmpty(war.OpponentTag)) // Bug in Supercell API
                    ret.Add(war);
            }

            return ret;
        }

        protected async Task<War> GetClanCurrentWar(string clanTag)
        {
            var url = string.Format("clans/{0}/currentwar", Uri.EscapeDataString(clanTag.ToUpperInvariant()));
            var data = await Request<War>(url);
            if (data != null)
                data.FixData(DateTime.MinValue);
            return data;
        }

        protected async Task<WarLeague> GetClanCurrentLeague(string clanTag)
        {
            try
            {
                var url = string.Format("clans/{0}/currentwar/leaguegroup", Uri.EscapeDataString(clanTag.ToUpperInvariant()));
                var data = await Request<WarLeague>(url);
                return data;
            }
            catch(Exception)
            {
                return null;
            }
        }

        public async Task<Clan> GetClan(string clanTag, bool withWarDetails, bool withCurrentWar)
        {
            var url = string.Format("clans/{0}", Uri.EscapeDataString(clanTag.ToUpperInvariant()));
            var data = await Request<Clan>(url);

            if (data.IsWarLogPublic && withWarDetails)
                data.Wars = await GetClanWarlog(clanTag);

            data.FixData();

            if (data.IsWarLogPublic && withCurrentWar)
            {
                var currentWar = await GetClanCurrentWar(clanTag);
                var league = await GetClanCurrentLeague(clanTag);

                data.InLeague = false;

                //Check if current war is one of the league opponents
                if (currentWar != null && league != null && league.Clans != null)
                {
                    foreach(var clan in league.Clans)
                    {
                        if (clan.Tag == currentWar.OpponentTag)
                            data.InLeague = true;
                    }
                    if (league.State == "inWar" && currentWar.State == "notInWar")
                        data.InLeague = true;
                }

                //Hide league status after one day of last war
                /*
                if(data.InLeague && currentWar != null && currentWar.EndTime < DateTime.UtcNow.AddDays(-1))
                {
                    data.InLeague = false;
                    currentWar = null;
                }
                */

                if (data.InLeague)
                    currentWar = null;

                if (currentWar != null)
                {
                    if (data.Wars == null)
                        data.Wars = new List<War>();

                    if (!string.IsNullOrEmpty(currentWar.State) && currentWar.State.Equals("warEnded"))
                    {
                        var prevWar = data.Wars.SingleOrDefault( w => 
                            w.EndTime >= currentWar.EndTime.AddMinutes(-10) && 
                            w.EndTime <= currentWar.EndTime.AddMinutes(10) &&
                            w.OpponentTag == currentWar.OpponentTag &&
                            w.TeamSize == currentWar.TeamSize
                            );
                        if (prevWar != null)
                        {
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
                        data.FixWars();
                    }
                }
            }

            return data;
        }

        public async Task<Player> GetPlayer(string playerTag)
        {
            var url = string.Format("players/{0}", Uri.EscapeDataString(playerTag.ToUpperInvariant()));
            var data = await Request<Player>(url);

            data.FixData();

            return data;
        }
    }
}
