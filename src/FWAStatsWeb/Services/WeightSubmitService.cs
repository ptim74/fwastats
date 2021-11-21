using FWAStatsWeb.Data;
using FWAStatsWeb.Logic;
using FWAStatsWeb.Models;
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
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace FWAStatsWeb.Services
{
    public class WeightSubmitService
    {
        private readonly ConcurrentQueue<SubmitEntry> q = new();
        private SubmitEntry currentEntry = null;
        private SubmitEntry previousEntry = null;

        private readonly ILogger<WeightSubmitService> logger;
        private readonly IOptions<WeightSubmitOptions> submitOptions;
        private readonly IHttpClientFactory clientFactory;

        private readonly IServiceScopeFactory scopeFactory;


        public WeightSubmitService(
            IServiceScopeFactory scopeFactory,
            ILogger<WeightSubmitService> logger,
            IOptions<WeightSubmitOptions> submitOptions,
            IHttpClientFactory clientFactory)
        {
            this.scopeFactory = scopeFactory;
            this.logger = logger;
            this.submitOptions = submitOptions;
            this.clientFactory = clientFactory;
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

        public async Task ProcessQueue()
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
            var changes = GetChangesCount(entry.Request);
            logger.LogInformation("Weight.SubmitChanges {0}", changes);
            if (changes < Constants.MIN_WEIGHT_CHANGES_ON_SUBMIT)
            {
                entry.Status.Message = "Too few weight changes since last submit.";
                logger.LogInformation("Weight.SubmitResponse {0}", entry.Status.Message);
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
                await UpdateOnSuccess(entry.Request);
        }

        protected int GetChangesCount(SubmitRequest request)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            int changes = 0;

            foreach (var m in request.Members)
            {
                var weight = db.Weights.Where(w => w.Tag == m.Tag).SingleOrDefault();
                if (weight == null || weight.SyncWeight != weight.WarWeight)
                    changes++;
            }

            return changes;
        }

        protected async Task UpdateOnSuccess(SubmitRequest request)
        {
            using var scope = scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            foreach (var m in request.Members)
            {
                var weight = db.Weights.Where(w => w.Tag == m.Tag).SingleOrDefault();
                if (weight != null)
                    weight.SyncWeight = weight.WarWeight;
            }

            await db.SaveChangesAsync();
        }

        protected async Task<SubmitResponse> NewSubmit(SubmitRequest request)
        {
            try
            {
                var client = clientFactory.CreateClient();
                var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
                var payloadText = JsonConvert.SerializeObject(request, settings);
                var stringContent = new StringContent(payloadText, Encoding.UTF8, "application/json");
                var responseMessage = await client.PostAsync(submitOptions.Value.SubmitURL, stringContent);
                responseMessage.EnsureSuccessStatusCode(); // throws if not 200-299
                string responseText = await responseMessage.Content.ReadAsStringAsync();
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
