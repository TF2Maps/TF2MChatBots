using System;
using System.Collections.Generic;
using System.IO;
using SteamKit2;
using Newtonsoft.Json;

namespace SteamBotLite
{
    class SearchModule : BaseModule
    {
        List<SearchClassEntry> Searches;

        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(Searches);            
        }

        public override void loadPersistentData()
        {
            try
            {
                Searches = JsonConvert.DeserializeObject<List<SearchClassEntry>>(File.ReadAllText(ModuleSavedDataFilePath()));
            }
            catch
            {
                Console.WriteLine("Error Loading SearchModule");
            }
        }
        public SearchModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            loadPersistentData();
            foreach (SearchClassEntry Entry in Searches)
                {
                commands.Add(new Search(bot, Entry));
                }
        }
        private class Search : BaseCommand
        {
            SearchClassEntry SearchData;
            public Search(VBot bot, SearchClassEntry SearchEntry) : base(bot,SearchEntry.Command)
            {
                this.SearchData = SearchEntry;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                return (SearchClass.Search(SearchData, param));
            }

        }
    }
}
