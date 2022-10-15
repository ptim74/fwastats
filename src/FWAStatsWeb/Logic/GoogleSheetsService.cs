using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FWAStatsWeb.Logic
{
    public interface IGoogleSheetsService
    {
        Task<IList<IList<object>>> Get(string sheetId, string majorDimension, string range);
        Task<IList<IList<IList<object>>>> BatchGet(string sheetId, string majorDimension, IList<string> ranges);
        Task Update(string sheetId, string majorDimension, string range, IList<IList<object>> values);
        Task BatchUpdate(string sheetId, string majorDimension, Dictionary<string, IList<IList<object>>> values);
    }

    public class GoogleSheetsService : GoogleBaseService, IGoogleSheetsService
    {
        public GoogleSheetsService(
            IOptions<GoogleServiceOptions> googleOptions) : base(googleOptions)
        {
        }

        private SheetsService sheetsService = null;

        protected SheetsService Service
        {
            get
            {
                sheetsService ??= new SheetsService(Initializer(new[] { SheetsService.Scope.Spreadsheets }));
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

        public async Task<IList<IList<IList<object>>>> BatchGet(string sheetId, string majorDimension, IList<string> ranges)
        {
            var getRequest = Service.Spreadsheets.Values.BatchGet(sheetId);
            getRequest.ValueRenderOption = SpreadsheetsResource.ValuesResource.BatchGetRequest.ValueRenderOptionEnum.UNFORMATTEDVALUE;
            getRequest.DateTimeRenderOption = SpreadsheetsResource.ValuesResource.BatchGetRequest.DateTimeRenderOptionEnum.SERIALNUMBER;
            getRequest.MajorDimension = (SpreadsheetsResource.ValuesResource.BatchGetRequest.MajorDimensionEnum)Enum.Parse(
                typeof(SpreadsheetsResource.ValuesResource.BatchGetRequest.MajorDimensionEnum), majorDimension);
            getRequest.Ranges = new Google.Apis.Util.Repeatable<string>(ranges);
            var getResponse = await getRequest.ExecuteAsync();

            var ret = new List<IList<IList<object>>>();
            foreach (var valueRange in getResponse.ValueRanges)
                ret.Add(valueRange.Values);
            return ret;
        }

        public async Task Update(string sheetId, string majorDimension, string range, IList<IList<object>> values)
        {
            var valueRange = new ValueRange { Values = values, MajorDimension = majorDimension };

            var updateRequest = Service.Spreadsheets.Values.Update(valueRange, sheetId, range);

            updateRequest.ValueInputOption =  SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
            updateRequest.IncludeValuesInResponse = false;

            await updateRequest.ExecuteAsync();
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
            await updateRequest.ExecuteAsync();
        }
    }
}
