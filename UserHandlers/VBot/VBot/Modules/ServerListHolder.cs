using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite.UserHandlers.VBot.VBot.Modules
{
    class ServerListHolder : BaseModule, ServerMapChangeListiner
    {
        ServerListHolder()
        {

        }
        private Dictionary<string, List<PlayEntry>> MapTests;

        class PlayEntry
        {
            string PlayerCount;
            string ServerIP;
            string TimeEntered;
            public PlayEntry(string playercount, string serverip, string timeentered)
            {
                this.PlayerCount = playercount;
                this.ServerIP = serverip;
                this.TimeEntered = timeentered;
            }
        }

        public void OnMapChange(ServerModule.ServerInfo args)
        {
            PlayEntry entry = new PlayEntry(args.playerCount.ToString(), args.serverIP, System.DateTime.Now.ToShortDateString() + " : " + System.DateTime.Now.ToShortTimeString());
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
                if (MapNameList.Contains(Item.Key).Equals(SummariseMethod)) //If we are using a whitelist, the bool will be "True" and if the list contains it, it'll be true, so it will add. Inversely for the blacklist.
                {
                    SumamrisedDictionary.Add(Item.Key, Item.Value.Count);
                }
            }
            return SumamrisedDictionary;
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
