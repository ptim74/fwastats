﻿using LWFStatsWeb.Data;
using LWFStatsWeb.Logic;
using LWFStatsWeb.Models;
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

        ILogger<WeightSubmitService> logger;
        IOptions<WeightSubmitOptions> submitOptions;
        IGoogleSheetsService googleSheets;
        IOptions<WeightResultOptions> resultDatabase;
        IClanLoader clanLoader;
        IOptions<GlobalOptions> globalOptions;

        public WeightSubmitService(
            ILogger<WeightSubmitService> logger,
            IOptions<WeightSubmitOptions> submitOptions,
            IGoogleSheetsService googleSheets,
            IOptions<WeightResultOptions> resultDatabase,
            IClanLoader clanLoader,
            IOptions<GlobalOptions> globalOptions)
        {
            this.logger = logger;
            this.submitOptions = submitOptions;
            this.googleSheets = googleSheets;
            this.resultDatabase = resultDatabase;
            this.clanLoader = clanLoader;
            this.globalOptions = globalOptions;
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
            if (globalOptions.Value.UseNewSubmitScript)
            {
                logger.LogInformation("Weight.SubmitRequest '{0}'", entry.Request.ClanName);
                entry.Status.UpdatePhase(SubmitPhase.Running);
                entry.Status.Message = "Calling Submit Script";
                entry.Request.Mode = "submit";
                var submitResponse = await NewSubmit(entry.Request);
                entry.Status.Message = submitResponse.ToString();
                logger.LogInformation("Weight.SubmitResponse {0}", submitResponse.ToString());
                if (submitResponse.Status)
                {
                    entry.Status.UpdatePhase(SubmitPhase.Succeeded);

                    entry.Request.Mode = "save";
                    var saveResponse = await NewSubmit(entry.Request);
                    logger.LogInformation("Weight.SaveResponse {0}", saveResponse.ToString());
                }
                else
                {
                    entry.Status.UpdatePhase(SubmitPhase.Failed);
                }
            }
            else
            {
                await OldSubmit(entry);
            }
        }

        protected async Task<SubmitResponse> NewSubmit(SubmitRequest request)
        {
            try
            {
                var webRequest = WebRequest.Create(globalOptions.Value.NewSubmitScript);
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

        protected async Task OldSubmit(SubmitEntry entry)
        {
            var request = entry.Request;
            var status = entry.Status;

            try
            {
                status.UpdatePhase(SubmitPhase.Running);
                
                //var response = entry.Response;
                var clanTag = request.ClanTag;
                var teamSize = request.Members.Count;

                logger.LogInformation("Started {0} [{1}]", clanTag, teamSize);

                var options = submitOptions.Value.SelectTeamSize(teamSize);
                var resultdb = resultDatabase.Value.SelectTeamSize(teamSize);
                var responseSheetId = options.SheetId;

                status.Message = "Reading FWA Clan List";
                await Task.Delay(TimeSpan.FromSeconds(2));

                string clanName = null;
                var clans = await clanLoader.Load(Constants.LIST_FWA);
                if (clans != null)
                {
                    var clan = clans.Where(c => c.Tag == clanTag).SingleOrDefault();
                    if (clan != null)
                    {
                        clanName = clan.Name;
                    }
                }

                if (string.IsNullOrEmpty(clanName))
                {
                    throw new Exception("Clan not found in FWA Clan List");
                }

                var nameSection = new List<object> { clanName, "", "", clanTag };

                var compositions = new Dictionary<int, int>();
                var weightSection = new List<object>();
                var tagSection = new List<object>();
                var thSection = new List<object>();

                for (int i = 0; i <= 11; i++)
                    compositions.Add(i, 0);

                foreach (var member in request.Members)
                {
                    compositions[member.TownHall]++;
                    weightSection.Add(member.Weight);
                    tagSection.Add(member.Tag);
                    thSection.Add(member.TownHall);
                }

                var compositionSection = new List<object>
                {
                    compositions[11],
                    compositions[10],
                    compositions[9],
                    compositions[8],
                    compositions[7] + compositions[6] + compositions[5] + compositions[4] + compositions[3]
                };

                var updateData = new Dictionary<string, IList<IList<object>>>
                {
                    { options.ClanNameRange, new List<IList<object>> { nameSection } },
                    { options.CompositionRange, new List<IList<object>> { compositionSection } },
                    { options.WeightRange, new List<IList<object>> { weightSection } },
                    { options.TagRange, new List<IList<object>> { tagSection } },
                    { options.THRange, new List<IList<object>> { thSection } }
                };

                status.Message = "Updating Submit Sheet";

                await googleSheets.BatchUpdate(options.SheetId, "COLUMNS", updateData);

                logger.LogInformation("Weight.SubmitRequest '{0}'", clanName);

                var checkStatus = false;

                try
                {
                    status.Message = "Calling Submit Script";
                    var submitRequest = WebRequest.Create(options.SubmitURL);
                    var submitResponse = await submitRequest.GetResponseAsync();

                    using (var reader = new StreamReader(submitResponse.GetResponseStream()))
                    {
                        var data = await reader.ReadToEndAsync();
                        try
                        {
                            
                            dynamic json = JsonConvert.DeserializeObject(data);
                            if(json is string)
                            {
                                //This is the value from StatusRange Cell
                                status.Message = json as string;
                                logger.LogInformation("Weight.SubmitResponse {0}", status.Message);
                            }
                            else
                            {
                                //Script error returned as json
                                if(json.message != null || json.name != null)
                                {
                                    checkStatus = true;
                                    string scriptError = string.Format("{0}: {1} (line {2} in '{3}')", json.name, json.message, json.lineNumber, json.fileName);
                                    logger.LogInformation("Weight.SubmitScript{0}", scriptError);
                                    status.Message = json.message;
                                }
                            }
                        }
                        catch(JsonReaderException)
                        {
                            //Script error returned as html
                            checkStatus = true;
                            logger.LogInformation("Weight.SubmitParsingError: {0}", data);
                        }
                    }
                }
                catch (WebException we)
                {
                    checkStatus = true;
                    logger.LogInformation("Weight.SubmitErrorHandler: {0}", we.Message);
                }

                if(checkStatus)
                {
                    try
                    {
                        status.Message = "Checking status";
                        var statusData = await googleSheets.Get(options.SheetId, "ROWS", options.StatusRange);
                        if (statusData != null && statusData.Count == 1 && statusData[0].Count == 1 && statusData[0][0] != null)
                        {
                            status.Message = statusData[0][0].ToString();
                        }
                        logger.LogInformation("Weight.SubmitCheckResponse {0}", status.Message);
                    }
                    catch (Exception e)
                    {
                        logger.LogError("Weight.SubmitCheckFailure: {0}", e.Message);
                    }
                }

                if (string.IsNullOrEmpty(status.Message))
                {
                    status.Message = "Unknown error";
                }

                if (status.Message.StartsWith(string.Format("Submitted '{0}'", clanName), StringComparison.OrdinalIgnoreCase))
                {
                    status.UpdatePhase(SubmitPhase.Succeeded);
                    status.Message = string.Format("Submitted '{0}'", clanName); //cut the timestamp
                }
                else
                {
                    status.UpdatePhase(SubmitPhase.Failed);
                }
            }
            catch (Exception e)
            {
                status.UpdatePhase(SubmitPhase.Failed);
                status.Message = e.Message;
                logger.LogError("Failed {0} {1}", request.ClanTag, e.ToString());
            }

        }
    }
}
