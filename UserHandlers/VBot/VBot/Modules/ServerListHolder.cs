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
        HTMLFileFromArrayListiners listiner;
        public ServerListHolder(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            loadPersistentData();
            MapNameList = new List<string>();
            List<string>Templist = JsonConvert.DeserializeObject<List<string>>(config["Whitelist"].ToString());

            if (Templist != null)
            {
                MapNameList = Templist;
            }

            if (bool.Parse(config["WhiteListIsBlacklist"].ToString()))
            {
                MethodToSummariseWith = SummariseMethod.Blacklist;
            }
            else
            {
                MethodToSummariseWith = SummariseMethod.Whitelist;
            }
            listiner = bot;
            
            loadPersistentData();
            UpdateList();

        }

        List<string> MapNameList;
        SummariseMethod MethodToSummariseWith;
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
            listiner.HTMLFileFromArray(new string[] { "MapName", "TimesPlayed" }, ParseSummarisedListToHTMLTable(SummariseEntries(MapTests, MapNameList, MethodToSummariseWith)), "ServerPlayedList");
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
        /// <param name="MapNameWhiteList"></param>
        /// <returns></returns>
        Dictionary<string, int> SummariseEntries(Dictionary<string, List<PlayEntry>> Dictionary, List<string> MapNameList , SummariseMethod MethodToSummariseWith)
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
                if ((MapNameList.Contains(Item.Key) == (SummariseMethod))) //If we are using a whitelist, the bool will be "True" and if the list contains it, it'll be true, so it will add. Inversely for the blacklist.
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
