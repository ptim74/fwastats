using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Options;
using System.Collections.Generic;

namespace FWAStatsWeb.Logic
{
    public class GoogleBaseService
    {
        private readonly IOptions<GoogleServiceOptions> googleOptions;

        public GoogleBaseService(
            IOptions<GoogleServiceOptions> googleOptions)
        {
            this.googleOptions = googleOptions;
        }

        protected BaseClientService.Initializer Initializer(IEnumerable<string> scopes)
        {
            return new BaseClientService.Initializer()
            {
                ApplicationName = googleOptions.Value.ApplicationName,
                HttpClientInitializer = new ServiceAccountCredential(
                    new ServiceAccountCredential.Initializer(googleOptions.Value.ClientEmail)
                    {
                        Scopes = scopes
                    }.FromPrivateKey(googleOptions.Value.PrivateKey))
            };
        }
    }
}
