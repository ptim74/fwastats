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
    public class ClanListOptions
    {
        public string Url { get; set; }
        public string Description { get; set; }
        public int TagColumn { get; set; }
        public int NameColumn { get; set; }
        public int DescriptionColumn { get; set; }
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

        protected async Task<string> LoadUrl()
        {
            var request = WebRequest.Create(options.Value.Url);
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
            var data = await LoadUrl();

            foreach (var row in data.Split('\n'))
            {
                var cells = row.Replace("\r", "").Split(',');

                string tag = null;
                string name = null;
                string desc = null;

                if (cells.Count() >= options.Value.TagColumn && options.Value.TagColumn > 0)
                    tag = cells[options.Value.TagColumn - 1];
                if (cells.Count() >= options.Value.NameColumn && options.Value.NameColumn > 0)
                    name = cells[options.Value.NameColumn - 1];
                if (cells.Count() >= options.Value.DescriptionColumn && options.Value.DescriptionColumn > 0)
                    desc = cells[options.Value.DescriptionColumn - 1];

                if (tag != null && tag.StartsWith("#"))
                {
                    tag = tag.ToUpperInvariant();
                    tag = tag.Replace("O", "0");
                    if (!tagList.Contains(tag))
                    {
                        tagList.Add(tag);
                        Objects.Add(new ClanObject { Tag = tag, Name = name, Description = desc });
                    }
                    else
                    {
                        Errors.Add(string.Format("Duplicate tag {0} for {1} in {2}", tag, name, options.Value.Description));
                    }
                }
            }

            return Objects;
        }
    }
}
