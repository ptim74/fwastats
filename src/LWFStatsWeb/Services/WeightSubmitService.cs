using LWFStatsWeb.Data;
using LWFStatsWeb.Logic;
using LWFStatsWeb.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LWFStatsWeb.Services
{
    public class WeightSubmitService
    {
        private ConcurrentQueue<SubmitEntry> q = new ConcurrentQueue<SubmitEntry>();
        private SubmitEntry currentEntry = null;
        private SubmitEntry previousEntry = null;

        private readonly ILogger<WeightSubmitService> logger;
        private readonly IOptions<WeightSubmitOptions> submitOptions;
        private readonly IGoogleSheetsService googleSheets;
        private readonly IOptions<WeightResultOptions> resultDatabase;
        private readonly IClanLoader clanLoader;

        private readonly IServiceScopeFactory scopeFactory;


        public WeightSubmitService(
            IServiceScopeFactory scopeFactory,
            ILogger<WeightSubmitService> logger,
            IOptions<WeightSubmitOptions> submitOptions,
            IGoogleSheetsService googleSheets,
            IOptions<WeightResultOptions> resultDatabase,
            IClanLoader clanLoader)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
            this.submitOptions = submitOptions;
            this.googleSheets = googleSheets;
            this.resultDatabase = resultDatabase;
            this.clanLoader = clanLoader;
        }

        public void Queue(SubmitRequest request)
        {
            logger.LogInformation("Queued {0} [{1}]", request.ClanTag, request.Members.Count);

            var entry = new SubmitEntry
            {
                Request = request,
                Status = new SubmitStatus
                {
                    Phase = SubmitPhase.Queued,
                    Timestamp = DateTime.UtcNow,
                    Message = "Queued"
                }
            };

            q.Enqueue(entry);
        }

        public SubmitStatus Status(string tag)
        {
            if (tag != null)
            {
                var entry = q.FirstOrDefault(e => e.Request.ClanTag == tag);
                if (entry != null)
                    return entry.Status;

                entry = currentEntry;
                if (entry != null && tag.Equals(entry.Request.ClanTag))
                    return entry.Status;

                entry = previousEntry;
                if (entry != null && tag.Equals(entry.Request.ClanTag))
                    return entry.Status;
            }
            return new SubmitStatus { Timestamp = DateTime.UtcNow, Message = "Unknown", Phase = SubmitPhase.Unknown };
        }

        public async Task ProcessQueue(CancellationToken cancellationToken)
        {
            while (q.TryDequeue(out SubmitEntry entry))
            {
                Interlocked.Exchange(ref currentEntry, entry);
                await Submit(entry);
                Interlocked.Exchange(ref previousEntry, entry);
            }
            Interlocked.Exchange(ref currentEntry, null);
        }

        protected async Task Submit(SubmitEntry entry)
        {
            logger.LogInformation("Weight.SubmitRequest '{0}'", entry.Request.ClanName);
            entry.Status.UpdatePhase(SubmitPhase.Running);
            if(GetChangesCount(entry.Request.ClanTag) < Constants.MIN_WEIGHT_CHANGES_ON_SUBMIT)
            {
                entry.Status.Message = "Too few weight changes since last submit.";
                entry.Status.UpdatePhase(SubmitPhase.Failed);
                return;
            }
            entry.Status.Message = "Calling Submit Script";
            entry.Request.Mode = "submit";
            var submitResponse = await NewSubmit(entry.Request);
            entry.Status.Message = submitResponse.ToString();
            logger.LogInformation("Weight.SubmitResponse {0}", submitResponse.ToString());
            var runningSecs = Convert.ToInt32(DateTime.UtcNow.Subtract(entry.Status.Timestamp).TotalSeconds);
            if (runningSecs > 15)
                logger.LogWarning("Submit took {0} seconds", runningSecs);
            var submitPhase = submitResponse.Status ? SubmitPhase.Succeeded : SubmitPhase.Failed;
            entry.Status.UpdatePhase(submitPhase);
            if (submitResponse.Status)
                await UpdateOnSuccess(entry.Request.ClanTag);
        }

        protected int GetChangesCount(string tag)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var q = from m in db.Members where m.ClanTag == tag join w in db.Weights on m.Tag equals w.Tag select w;
                int changes = 0;
                foreach (var w in q)
                    if (w.WarWeight != w.SyncWeight)
                        changes++;
                return changes;
            }
        }

        protected async Task UpdateOnSuccess(string tag)
        {
            using (var scope = scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var q = from m in db.Members where m.ClanTag == tag join w in db.Weights on m.Tag equals w.Tag select w;
                foreach (var w in q)
                    w.SyncWeight = w.WarWeight;

                await db.SaveChangesAsync();
            }
        }

        protected async Task<SubmitResponse> NewSubmit(SubmitRequest request)
        {
            try
            {
                var webRequest = WebRequest.Create(submitOptions.Value.SubmitURL);
                webRequest.Method = "POST";
                var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var payloadText = JsonConvert.SerializeObject(request, settings);
                byte[] postBytes = Encoding.UTF8.GetBytes(payloadText);
                webRequest.ContentLength = postBytes.Length;
                Stream requestStream = await webRequest.GetRequestStreamAsync();
                await requestStream.WriteAsync(postBytes, 0, postBytes.Length);
                requestStream.Close();
                string responseText = null;
                var webResponse = await webRequest.GetResponseAsync();
                using (var reader = new StreamReader(webResponse.GetResponseStream()))
                {
                    responseText = await reader.ReadToEndAsync();
                }

                var response = JsonConvert.DeserializeObject<SubmitResponse>(responseText);
                return response;
            }
            catch (Exception e)
            {
                logger.LogError("Failed {0} {1}", request.ClanTag, e.ToString());
                return new SubmitResponse
                {
                    Status = false,
                    Details = e.Message
                };
            }
        }
    }
}
