using LWFStatsWeb.Logic;
using LWFStatsWeb.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LWFStatsWeb.Services
{
    public interface IWeightSubmitService
    {
        void Queue(WeightResult result);
        SubmitResult Status(string tag);
    }

    public class WeightSubmitService : HostedService, IWeightSubmitService
    {
        private ConcurrentQueue<WeightResult> q = new ConcurrentQueue<WeightResult>();
        private WeightResult currentResult = null;
        private WeightResult previousResult = null;

        ILogger<WeightSubmitService> logger;
        IOptions<WeightSubmitOptions> submitOptions;
        IGoogleSheetsService googleSheets;
        IOptions<WeightResultOptions> resultDatabase;
        IClanLoader clanLoader;

        public WeightSubmitService(
            ILogger<WeightSubmitService> logger,
            IOptions<WeightSubmitOptions> submitOptions,
            IGoogleSheetsService googleSheets,
            IOptions<WeightResultOptions> resultDatabase,
            IClanLoader clanLoader)
        {
            this.logger = logger;
            this.submitOptions = submitOptions;
            this.googleSheets = googleSheets;
            this.resultDatabase = resultDatabase;
            this.clanLoader = clanLoader;
        }

        public void Queue(WeightResult result)
        {
            /*
            logger.LogInformation("Queued {0}", result.LinkID);
            
            result.SubmitMessage = "Queued";
            result.SubmitResult = false;
            result.SubmitProcessed = false;
            result.Timestamp = DateTime.UtcNow;

            q.Enqueue(result);
            */
        }

        public SubmitResult Status(string tag)
        {
            /*
            if (tag != null)
            {
                var result = q.FirstOrDefault(r => r.Tag == tag);
                if (result != null)
                    return new SubmitResult { Timestamp = result.Timestamp, Message = result.SubmitMessage, State = SubmitState.Queued };

                result = currentResult;
                if (result != null && tag.Equals(result.Tag))
                    return new SubmitResult { Timestamp = result.Timestamp, Message = result.SubmitMessage, State = SubmitState.Running };

                result = previousResult;
                if (result != null && tag.Equals(result.Tag))
                    return new SubmitResult { Timestamp = result.Timestamp, Message = result.SubmitMessage, State = result.SubmitResult ? SubmitState.Succeeded : SubmitState.Failed };
            }
            */
            return new SubmitResult { Timestamp = DateTime.UtcNow, Message = "Not found", State = SubmitState.Unknown };
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await RunAsync(cancellationToken);
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
        }

        protected async Task RunAsync(CancellationToken cancellationToken)
        {
            while (q.TryDequeue(out WeightResult result))
            {
                Interlocked.Exchange(ref currentResult, result);
                await Submit(result);
                Interlocked.Exchange(ref previousResult, result);
            }
            Interlocked.Exchange(ref currentResult, null);
        }

        public async Task Submit(WeightResult result)
        {
            try
            {
                //logger.LogInformation("Started {0} [{1}]", result.LinkID, result.TeamSize);

                var options = submitOptions.Value.SelectTeamSize(result.TeamSize);
                var resultdb = resultDatabase.Value.SelectTeamSize(result.TeamSize);
                var responseSheetId = options.SheetId;

                //result.SubmitMessage = "Reading FWA Clan List";
                await Task.Delay(TimeSpan.FromSeconds(2));

                string clanName = null;
                var clans = await clanLoader.Load(Constants.LIST_FWA);
                if (clans != null)
                {
                    var clan = clans.Where(c => c.Tag == result.Tag).SingleOrDefault();
                    if (clan != null)
                    {
                        clanName = clan.Name;
                    }
                }

                if (string.IsNullOrEmpty(clanName))
                {
                    throw new Exception("Clan not found in FWA Clan List");
                }

                var nameSection = new List<object> { clanName, "", "", result.Tag };

                var compositions = new Dictionary<int, int>();
                var weightSection = new List<object>();
                var tagSection = new List<object>();
                var thSection = new List<object>();

                for (int i = 0; i <= 11; i++)
                    compositions.Add(i, 0);


                /*
                foreach (var member in result.)
                {
                    compositions[member.TownHallLevel]++;
                    weightSection.Add(member.Weight);
                    tagSection.Add(member.Tag);
                    thSection.Add(member.TownHallLevel);
                }*/

                /*


                    

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

                    await googleSheets.BatchUpdate(options.SheetId, "COLUMNS", updateData);

                    logger.LogInformation("Weight.SubmitRequest '{0}'", clanName);

                    var checkStatus = false;

                    try
                    {
                        var submitRequest = WebRequest.Create(options.SubmitURL);
                        submitRequest.Timeout = 15000;
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
                                    model.Message = json as string;
                                    logger.LogInformation("Weight.SubmitResponse {0}", model.Message);
                                }
                                else
                                {
                                    //Script error returned as json
                                    if(json.message != null || json.name != null)
                                    {
                                        checkStatus = true;
                                        string scriptError = string.Format("{0}: {1} (line {2} in '{3}')", json.name, json.message, json.lineNumber, json.fileName);
                                        logger.LogInformation("Weight.SubmitScript{0}", scriptError);
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
                            var statusData = await googleSheets.Get(options.SheetId, "ROWS", options.StatusRange);
                            if (statusData != null && statusData.Count == 1 && statusData[0].Count == 1 && statusData[0][0] != null)
                            {
                                model.Message = statusData[0][0].ToString();
                            }
                            logger.LogInformation("Weight.SubmitCheckResponse {0}", model.Message);
                        }
                        catch (Exception e)
                        {
                            logger.LogError("Weight.SubmitCheckFailure: {0}", e.Message);
                        }
                    }

                    if (string.IsNullOrEmpty(model.Message))
                    {
                        model.Message = "Unknown error";
                    }
                    else
                    {
                        if (model.Message.Equals(string.Format("Submitted '{0}'", clanName), StringComparison.OrdinalIgnoreCase))
                        {
                            model.Status = true;
                            responseSheetId = results.SheetId;
                            //await this.UpdatePendingSubmit(weight.Members.Count, weight.ClanTag);
                            var result = db.WeightResults.SingleOrDefault(r => r.Tag == weight.ClanTag);
                            if (result == null)
                            {
                                result = new WeightResult { Tag = weight.ClanTag, Timestamp = DateTime.MinValue };
                                db.WeightResults.Add(result);
                            }
                            result.PendingResult = true;
                            db.SaveChanges();
                        }
                    }
                }
                catch(Exception e)
                {
                    model.Message = e.Message;
                    logger.LogError("Weight.SubmitError {0}", e.ToString());
                }
                model.SheetUrl = $"https://docs.google.com/spreadsheets/d/{responseSheetId}";
                return View("WeightSubmit", model);
                 */

                //result.SubmitMessage = "OK";

                //logger.LogInformation("Finished {0} {1}", result.LinkID, result.SubmitMessage);

            }
            catch (Exception)
            {
                //result.SubmitMessage = e.Message;
                //logger.LogError("Failed {0} {1}", result.LinkID, e.ToString());
            }
            finally
            {
                //result.SubmitProcessed = true;
            }
        }
    }
}
