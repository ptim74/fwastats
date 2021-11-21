using FWAStatsWeb.Models;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace FWAStatsWeb.Logic
{
    public class ClanListOptions : List<ClanListDetails>
    {
    }

    public class ClanListDetails
    {
        public string SheetId { get; set; }
        public string Range { get; set; }
        public string Code { get; set; }
        public int TagColumn { get; set; }
        public int NameColumn { get; set; }
    }

    public interface IClanLoader
    {
        ICollection<string> Errors { get; set; }

        Task<ICollection<ClanObject>> Load(string listName);
    }

    public class ClanLoader : IClanLoader
    {
        private readonly IOptions<ClanListOptions> options;
        private readonly IGoogleSheetsService googleSheets;

        public ClanLoader(
            IOptions<ClanListOptions> options,
            IGoogleSheetsService googleSheets)
        {
            this.options = options;
            this.googleSheets = googleSheets;
        }

        public ICollection<string> Errors { get; set; }

        private ICollection<ClanObject> Objects { get; set; }

        public async Task<ICollection<ClanObject>> Load(string listName)
        {
            Errors = new List<string>();
            Objects = new List<ClanObject>();

            var tagDict = new Dictionary<string, string>();

            foreach(var listOptions in options.Value.Where(l => l.Code == listName))
            {
                var data = await googleSheets.Get(listOptions.SheetId, "ROWS", listOptions.Range);

                if(data != null)
                {
                    foreach(var row in data)
                    {
                        string tag = null;
                        string name = null;

                        if (row.Count > listOptions.TagColumn && listOptions.TagColumn >= 0)
                            tag = (string)row[listOptions.TagColumn];
                        if (row.Count > listOptions.NameColumn && listOptions.NameColumn >= 0)
                            name = Convert.ToString(row[listOptions.NameColumn]);

                        if (tag != null && tag.StartsWith("#"))
                        {
                            tag = tag.ToUpperInvariant();
                            tag = tag.Replace("O", "0");
                            if (tagDict.TryGetValue(tag, out string existingName))
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
                }

                if (Objects.Count == 0)
                {
                    Errors.Add(string.Format("{0}-list is empty", listOptions.Code));
                }

            }

            return Objects;
        }
    }
}
