using LWFStatsWeb.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class ClanListOptions : List<ClanListDetails>
    {
    }

    public class ClanListDetails
    {
        public string Url { get; set; }
        public string Code { get; set; }
        public int TagColumn { get; set; }
        public int NameColumn { get; set; }
    }

    public interface IClanLoader
    {
        List<string> Errors { get; set; }

        Task<List<ClanObject>> Load(string listName);
    }

    public class ClanLoader : IClanLoader
    {
        IOptions<ClanListOptions> options;

        public ClanLoader(IOptions<ClanListOptions> options)
        {
            this.options = options;
        }

        public List<string> Errors { get; set; }

        private List<ClanObject> Objects { get; set; }

        protected async Task<string> LoadUrl(string url)
        {
            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                return await reader.ReadToEndAsync();
            }
        }

        public async Task<List<ClanObject>> Load(string listName)
        {
            Errors = new List<string>();
            Objects = new List<ClanObject>();

            var tagDict = new Dictionary<string, string>();

            foreach(var listOptions in options.Value.Where(l => l.Code == listName))
            {
                var data = await LoadUrl(listOptions.Url);

                var countBefore = Objects.Count();

                foreach (var row in data.Split('\n'))
                {
                    var cells = row.Replace("\r", "").Split(',');

                    string tag = null;
                    string name = null;

                    if (cells.Count() >= listOptions.TagColumn && listOptions.TagColumn > 0)
                        tag = cells[listOptions.TagColumn - 1];
                    if (cells.Count() >= listOptions.NameColumn && listOptions.NameColumn > 0)
                        name = cells[listOptions.NameColumn - 1];

                    if(tag != null && tag.StartsWith("\"") && tag.EndsWith("\""))
                    {
                        tag = tag.Substring(1, tag.Length -2);
                        tag = tag.Replace("\"\"", "\"");
                    }

                    if (tag != null && tag.StartsWith("#"))
                    {
                        tag = tag.ToUpperInvariant();
                        tag = tag.Replace("O", "0");
                        string existingName;
                        if(tagDict.TryGetValue(tag,out existingName))
                        {
                            Errors.Add(string.Format("Duplicate tag {0} for {1} ({2}) and {3}", tag, name, listOptions.Code, existingName));
                        }
                        else
                        {
                            tagDict.Add(tag, string.Format("{0} ({1})", name, listOptions.Code));
                            Objects.Add(new ClanObject { Tag = tag, Name = name, Group = listOptions.Code });
                        }
                    }
                }

                if(Objects.Count() == countBefore)
                {
                    Errors.Add(string.Format("{0}-list is empty",listOptions.Code));
                }
            }

            return Objects;
        }
    }
}
