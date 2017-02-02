using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    class ServerListHolder : BaseModule, ServerMapChangeListiner
    {

        class Maplist
        {
            public List<string> Maps;
            public SummariseMethod ListKind;
            public string ListName;
        }



        string[] Header = new string[] { "Map", "Time Played" };
        HTMLFileFromArrayListiners listiner;

        public ServerListHolder(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            loadPersistentData();

            List<Maplist> Maplist = new List<Maplist>();

            List<Maplist> Templist = new List<Maplist>();
           
            Templist = JsonConvert.DeserializeObject<List<Maplist>>(config["ListConfigs"].ToString());

            if (Templist != null)
            {
                this.Maplists = Templist;
            }
            
            listiner = bot;
            loadPersistentData();
            UpdateList();

            bot.MapChangeEventListiners.Add(this);
        }

        public override void OnAllModulesLoaded()
        {

        }

        List<Maplist> Maplists;
        private Dictionary<string, List<PlayEntry>> MapTests;

        public class PlayEntry
        {
            public string PlayerCount {get; set;}
            public string ServerIP { get; set; }
            public string TimeEntered { get; set; }
            public PlayEntry(string playercount, string serverip, string timeentered)
            {
                this.PlayerCount = playercount;
                this.ServerIP = serverip;
                this.TimeEntered = timeentered;
            }
        }

        public void OnMapChange(ServerModule.ServerInfo args)
        {
          //  Tuple<string,string,string> entry = new Tuple<string, string, string>()
            PlayEntry entry = new PlayEntry(args.playerCount.ToString(), args.serverIP, System.DateTime.Now.ToShortDateString() + " : " + System.DateTime.Now.ToShortTimeString());
            AddEntry(args.currentMap, entry);
            savePersistentData();
            UpdateList();
            
        }

        void UpdateList ()
        {
            foreach(Maplist entry in Maplists)
            {
                listiner.HTMLFileFromArray(Header, ParseSummarisedListToHTMLTable(SummariseEntries(MapTests, entry.Maps, entry.ListKind)), entry.ListName);
            }
            
        }

        void AddEntry(string MapName, PlayEntry Entry)
        {
            if (MapTests.ContainsKey(MapName))
            {
                MapTests[MapName].Add(Entry);
            }
            else
            {
                MapTests.Add(MapName, new List<PlayEntry>() { Entry });
            }
        }


        enum SummariseMethod { Blacklist,Whitelist}

        /// <summary>
        /// Used to summarise the Dictionary but only return entries in the whitelist or not in the blacklist
        /// </summary>
        /// <param name="Dictionary"></param>
        /// <param name="Filter"></param>
        /// <returns></returns>
        Dictionary<string, int> SummariseEntries(Dictionary<string, List<PlayEntry>> Dictionary, List<string> Filter , SummariseMethod MethodToSummariseWith)
        {
            //Assign Values for the Boolean 
            bool SummariseMethod;

            if (MethodToSummariseWith.Equals(ServerListHolder.SummariseMethod.Whitelist))
            {
                SummariseMethod = true; 
            }
            else
            {
                SummariseMethod = false;
            }

            Dictionary<string, int> SumamrisedDictionary = new Dictionary<string, int>();

            foreach (KeyValuePair<string, List<PlayEntry>> Item in Dictionary)
            {
                if ((Filter.Contains(Item.Key) == (SummariseMethod))) //If we are using a whitelist, the bool will be "True" and if the list contains it, it'll be true, so it will add. Inversely for the blacklist.
                {
                    SumamrisedDictionary.Add(Item.Key, Item.Value.Count);
                }
            }
            return SumamrisedDictionary;
        }
        List<string[]> ParseSummarisedListToHTMLTable (Dictionary<string,int> Dictionary)
        {
            List<string[]> Array = new List<string[]>();
            foreach(KeyValuePair<string,int> entry in Dictionary)
            {
                Array.Add(new string[] { entry.Key, entry.Value.ToString() });
            }
            return Array;
        }

        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(MapTests);
        }

        public override void loadPersistentData()
        {
            if (File.Exists(ModuleSavedDataFilePath()))
            {
                Console.WriteLine("Loading Map Play Entries");
                MapTests = JsonConvert.DeserializeObject<Dictionary<string, List<PlayEntry>>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                Console.WriteLine("Loaded Map Play Entries");
            }
            else
            {
                Console.WriteLine("Loading Map Play Entries");
                MapTests = new Dictionary<string, List<PlayEntry>>();
                
            }
        }
    }
}
