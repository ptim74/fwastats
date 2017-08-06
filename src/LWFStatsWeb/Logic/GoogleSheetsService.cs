using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public interface IGoogleSheetsService
    {
        Task<IList<IList<object>>> Get(string sheetId, string majorDimension, string range);
        Task Update(string sheetId, string majorDimension, string range, IList<IList<object>> values);
        Task BatchUpdate(string sheetId, string majorDimension, Dictionary<string, IList<IList<object>>> values);
    }

    public class GoogleSheetsService : IGoogleSheetsService
    {
        IOptions<GoogleServiceOptions> googleOptions;

        public GoogleSheetsService(
            IOptions<GoogleServiceOptions> googleOptions)
        {
            this.googleOptions = googleOptions;
        }

        private SheetsService sheetsService = null;

        protected SheetsService Service
        {
            get
            {
                if(sheetsService == null)
                {
                    sheetsService = new SheetsService(
                        new BaseClientService.Initializer()
                        {
                            ApplicationName = googleOptions.Value.ApplicationName,
                            HttpClientInitializer = new ServiceAccountCredential(
                                new ServiceAccountCredential.Initializer(googleOptions.Value.ClientEmail)
                                {
                                    Scopes = new[] { SheetsService.Scope.Spreadsheets }
                                }.FromPrivateKey(googleOptions.Value.PrivateKey))
                        });
                }
                return sheetsService;
            }
        }

        public async Task<IList<IList<object>>> Get(string sheetId, string majorDimension, string range)
        {
            var getRequest = Service.Spreadsheets.Values.Get(sheetId, range);
            getRequest.ValueRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
            getRequest.DateTimeRenderOption = SpreadsheetsResource.ValuesResource.GetRequest.DateTimeRenderOptionEnum.SERIALNUMBER;
            getRequest.MajorDimension = (SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum)Enum.Parse(
                typeof(SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum), majorDimension);
            var getResponse = await getRequest.ExecuteAsync();
            return getResponse.Values;
        }

        public async Task Update(string sheetId, string majorDimension, string range, IList<IList<object>> values)
        {
            var valueRange = new ValueRange { Values = values, MajorDimension = majorDimension };

            var updateRequest = Service.Spreadsheets.Values.Update(valueRange, sheetId, range);

            updateRequest.ValueInputOption =  SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            updateRequest.IncludeValuesInResponse = false;

            var updateResponse = await updateRequest.ExecuteAsync();
        }

        public async Task BatchUpdate(string sheetId, string majorDimension, Dictionary<string, IList<IList<object>>> values)
        {
            var updateRequestBody = new BatchUpdateValuesRequest {
                Data = new List<ValueRange>(),
                ValueInputOption = "RAW",
                IncludeValuesInResponse = false
            };

            foreach(var data in values)
            {
                updateRequestBody.Data.Add(
                    new ValueRange() {
                        MajorDimension = majorDimension,
                        Range = data.Key,
                        Values = data.Value
                    });
            }

            var updateRequest = Service.Spreadsheets.Values.BatchUpdate(updateRequestBody, sheetId);
            var updateResponse = await updateRequest.ExecuteAsync();
        }
    }
}
