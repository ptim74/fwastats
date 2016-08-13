using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LWFStatsWeb.Logic
{
    public class MockClanLoader : IClanLoader
    {
        public List<string> Errors { get; set; }

        public Task<List<ClanObject>> Load()
        {
            Errors = new List<string>();

            var list = new List<ClanObject>();

            //list.Add(new ClanObject { Tag = "#YPJ28YJG", Name = "War Farmers 01" });
            list.Add(new ClanObject { Tag = "#8C80VQ2Y", Name = "War Farmers 02" });
            list.Add(new ClanObject { Tag = "#YVVJ00Y8", Name = "War Farmers 04" });
            list.Add(new ClanObject { Tag = "#Y8VGCVYC", Name = "War Farmers 06" });
            list.Add(new ClanObject { Tag = "#YRRUCQRR", Name = "War Farmers 07" });
            list.Add(new ClanObject { Tag = "#PPRRU8QQ", Name = "War Farmers 09" });
            list.Add(new ClanObject { Tag = "#Y9Q9JRCP", Name = "War Farmers 11" });
            list.Add(new ClanObject { Tag = "#82LY29GL", Name = "War Farmers 12" });
            list.Add(new ClanObject { Tag = "#LGYC98QY", Name = "War Farmers 13" });
            list.Add(new ClanObject { Tag = "#LGYCCYLQ", Name = "War Farmers 14" });
            list.Add(new ClanObject { Tag = "#LQGRJCPR", Name = "War Farmers 15" });
            list.Add(new ClanObject { Tag = "#L929PGQ2", Name = "War Farmers 16" });
            //list.Add(new ClanObject { Tag = "#8CPGGJ8P", Name = "War Farmers 17" });
            //list.Add(new ClanObject { Tag = "#LGQJYLPY", Name = "War Farmers 18" });
            list.Add(new ClanObject { Tag = "#LLPP82CJ", Name = "War Farmers 19" });
            list.Add(new ClanObject { Tag = "#2P2P8JJG", Name = "War Farmers 21" });
            list.Add(new ClanObject { Tag = "#LG9J8Q22", Name = "War Farmers 22" });
            list.Add(new ClanObject { Tag = "#LJ2CPG28", Name = "War Farmers 25" });
            list.Add(new ClanObject { Tag = "#LCPJL298", Name = "War Farmers 26" });
            list.Add(new ClanObject { Tag = "#LLPCJRYP", Name = "War Farmers 27" });

            foreach (var clan in list)
                clan.Group = "MOCK";

            return Task.FromResult(list);
        }
    }
}
