using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LWFStatsJob
{
    public class Program
    {
        public string LWFStatsURL
        {
            get
            {
                return "http://lwfstats.azurewebsites.net";
            }
        }

        private async Task<string> Request(string page)
        {
            var url = string.Format("{0}/{1}", LWFStatsURL, page);
            var request = WebRequest.Create(url);
            request.ContentType = "application/json; charset=utf-8";
            var response = await request.GetResponseAsync();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                var data = reader.ReadToEnd();
                return data;
            }
        }

        private async Task<T> Request<T>(string page)
        {
            var pageData = await Request(page);
            return JsonConvert.DeserializeObject<T>(pageData);
        }

        public async Task<int> Run()
        {
            var failures = 0;

            Log(string.Format("Run started, connecting to {0}", LWFStatsURL));
            var index = await Request<UpdateIndexView>("Update/GetTasks");

            if (index != null)
            {
                Log("Processing list");
                foreach (var e in index.Errors)
                {
                    Log(e);
                }
                foreach (var task in index.Tasks)
                {
                    var status = await Request<TaskStatus>(string.Format("Update/UpdateTask/{0}", task.ID));
                    if (status != null)
                    {
                        Log(string.Format("{0}: {1}", status.Message, status.Status));
                        if (!status.Status)
                            failures++;
                    }
                }
                var finish = await Request<TaskStatus>("Update/UpdateFinished/");
                if (finish != null)
                {
                    Log(string.Format("{0}: {1}", finish.Message, finish.Status));
                }

            }
            Log(string.Format("Run finished, {0} failures", failures));
            return failures;
        }

        public void Log(string message)
        {
            Console.WriteLine(message);
        }

        static int Main(string[] args)
        {
            return new Program().Run().Result;
        }
    }
}
