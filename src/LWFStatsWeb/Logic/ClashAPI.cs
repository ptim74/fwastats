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
                Exception ret = e;
                try
                {
                    using (var reader = new StreamReader(e.Response.GetResponseStream())) //TODO: NullReferenceException
                    {
                        var data = await reader.ReadToEndAsync();
                        var error = JsonConvert.DeserializeObject<ClashApiError>(data);
                        if (error != null)
                        {
                            var msg = $"API Error {e.Status}, Reason: {error.Reason}, Message: {error.Message}";
                            ret = new Exception(msg, e);
                        }
                    }
                }
                catch (Exception) {}
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
            var url = string.Format("clans/{0}/warlog", Uri.EscapeDataString(clanTag));
            var data = await Request<Warlog>(url);
            if (data == null)
                return null;
            return data.Wars;
        }

        protected async Task<War> GetClanCurrentWar(string clanTag)
        {
            var url = string.Format("clans/{0}/currentwar", Uri.EscapeDataString(clanTag));
            var data = await Request<War>(url);
            if (data != null)
                data.FixData(DateTime.MinValue);
            return data;
        }

        protected async Task<WarLeague> GetClanCurrentLeague(string clanTag)
        {
            try
            {
                var url = string.Format("clans/{0}/currentwar/leaguegroup", Uri.EscapeDataString(clanTag));
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
            var url = string.Format("clans/{0}", Uri.EscapeDataString(clanTag));
            var data = await Request<Clan>(url);

            if (data.IsWarLogPublic && withWarDetails)
                data.Wars = await GetClanWarlog(clanTag);

            data.FixData();

            if (data.IsWarLogPublic && withCurrentWar)
            {
                var currentWar = await GetClanCurrentWar(clanTag);
                var league = await GetClanCurrentLeague(clanTag);

                data.InLeague = false;

                if (currentWar != null && league != null && league.Clans != null)
                {
                    foreach(var clan in league.Clans)
                    {
                        if (clan.Tag == currentWar.OpponentTag)
                            data.InLeague = true;
                    }
                }

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
            var url = string.Format("players/{0}", Uri.EscapeDataString(playerTag));
            var data = await Request<Player>(url);

            data.FixData();

            return data;
        }
    }
}
