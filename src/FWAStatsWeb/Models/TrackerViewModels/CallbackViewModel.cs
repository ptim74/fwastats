using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Models.TrackerViewModels
{
    public class CallbackViewModel
    {
        public string TokenType { get; set; }
        public string AccessToken { get; set; }
        public string Scope { get; set; }
        public long ExpiresIn { get; set; }
        public string RefreshToken { get; set; }
        public string State { get; set; }
        public CallbackWebhook Webhook { get; set; }
    }

    public class CallbackWebhook
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string ChannelId { get; set; }
        public string Token { get; set; }
        public string Avatar { get; set; }
        public string GuildId { get; set; }
        public string Id { get; set; }
        public string State { get; set; }
    }
}
