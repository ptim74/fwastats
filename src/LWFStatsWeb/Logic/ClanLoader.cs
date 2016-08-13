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

        Task<List<ClanObject>> Load();
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
                return reader.ReadToEnd();
            }
        }

        public async Task<List<ClanObject>> Load()
        {
            Errors = new List<string>();
            Objects = new List<ClanObject>();

            var tagList = new List<string>();

            foreach(var listOptions in options.Value)
            {
                var data = await LoadUrl(listOptions.Url);

                foreach (var row in data.Split('\n'))
                {
                    var cells = row.Replace("\r", "").Split(',');

                    string tag = null;
                    string name = null;

                    if (cells.Count() >= listOptions.TagColumn && listOptions.TagColumn > 0)
                        tag = cells[listOptions.TagColumn - 1];
                    if (cells.Count() >= listOptions.NameColumn && listOptions.NameColumn > 0)
                        name = cells[listOptions.NameColumn - 1];

                    if (tag != null && tag.StartsWith("#"))
                    {
                        tag = tag.ToUpperInvariant();
                        tag = tag.Replace("O", "0");
                        if (!tagList.Contains(tag))
                        {
                            tagList.Add(tag);
                            Objects.Add(new ClanObject { Tag = tag, Name = name, Group = listOptions.Code });
                        }
                        else
                        {
                            Errors.Add(string.Format("Duplicate tag {0} for {1} in {2}", tag, name, listOptions.Code));
                        }
                    }
                }
            }

            return Objects;
        }
    }
}
