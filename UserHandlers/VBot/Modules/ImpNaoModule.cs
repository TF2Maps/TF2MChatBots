
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;
using SteamKit2;
using Newtonsoft.Json;
using System.Linq;
namespace SteamBotLite
{
    class ImpNaoModule : BaseModule
    {
        
        public ImpNaoModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            loadPersistentData();
            
            commands.Add(new Add(bot, this));
            commands.Add(new Maps(bot, this));
            commands.Add(new Delete(bot, this));/*
            commands.Add(new Update(bot, this));
            adminCommands.Add(new Wipe(bot, this));
            */
        }

        public class ImpNaoMap
        {
            public string Submitter { get; set; }
            public string name { get; set; }
            public string link { get; set; }
            public string Notes { get; set; }
            public string password { get; set; }
        }

        public override string getPersistentData()
        {
            return null;
        }

        public override void loadPersistentData()
        {
        }

        public void HandleEvent(object sender, ServerModule.ServerInfo args)
        {
        }

        // The abstract command for motd

        abstract public class MapCommand : BaseCommand
        {
            protected ImpNaoModule ImpNaoModule;

            public MapCommand(VBot bot, string command, ImpNaoModule mapMod)
                : base(bot, command)
            {
                this.ImpNaoModule = mapMod;
            }
        }
        

        // The commands

        private class Add : MapCommand
        {
            ImpNaoModule impnaomodule;
            public bool uploadcheck(string MapName, string Website)
            {
                return SearchClass.CheckDataExistsOnWebPage(Website, MapName); //TODO develop method to check website
            }

            public Add(VBot bot, ImpNaoModule Impnaomodule) : base(bot, "!impadd", Impnaomodule) {
                impnaomodule = Impnaomodule;
            }

            protected override string exec(SteamID sender, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);

                if (parameters.Length == 2)
                {
                    ImpNaoMap Map = new ImpNaoMap();
                    Map.name = parameters[0];
                    Map.link = parameters[1];
                    Map.password = sender.ToString();
                    impnaomodule.AddMapOnline(Map, "Body");
                }
                else
                {
                    return "Invalid Syntax";
                }


                
                return "Completed";
            }
        }

        public async Task AddMapOnline(ImpNaoMap Map,string Body)
        {
            using (var client = new HttpClient())
            {

                HttpContent Content = new StringContent(JsonConvert.SerializeObject(Map));
                var response = await client.PostAsync("http://carbidegames.com/impnao/api/maps", Content);
                

                var responseString = await response.Content.ReadAsStringAsync();
            }
        }

        public async Task DeleteMapOnline(string Map)
        {
            using (var client = new HttpClient())
            {
                
                HttpContent Content = new StringContent((Map));

                string url = "http://carbidegames.com/impnao/api/maps/" + Map;

                var response = await client.DeleteAsync("http://carbidegames.com/impnao/api/maps/" + Map);

                var responseString = await response.Content.ReadAsStringAsync();
            }
        }

        private class Maps : MapCommand
        {
            public Maps(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!impmaps", impnaomodule) { }
            protected override string exec(SteamID sender, string param)
            {
                string ImpNaoPage = SearchClass.GetWebPageAsString("http://carbidegames.com/impnao/api/maps");
                List <ImpNaoMap> mapList = JsonConvert.DeserializeObject<List<ImpNaoMap>>(ImpNaoPage);

                string chatResponse = "";
                string pmResponse = "";
                int maxMaps = 10;

                chatResponse = string.Join(" , ", mapList.Select(x => x.name));
                if (mapList.Count > maxMaps)
                    chatResponse += string.Format(" (and {0} more...)", mapList.Count - maxMaps);

                // Build the private response.
                pmResponse = "";
                for (int i = 0; i < mapList.Count; i++)
                {
                    string mapLine = string.Format("{0} // {1} // {2} ({3})", mapList[i].name, mapList[i].link, userhandler.steamConnectionHandler.SteamFriends.GetFriendPersonaName(new SteamID(mapList[i].Submitter)), mapList[i].Submitter);

                    if (!string.IsNullOrEmpty(mapList[i].Notes))
                        mapLine += "\nNotes: " + mapList[i].Notes;

                    if (i < mapList.Count - 1)
                        mapLine += "\n";

                    pmResponse += mapLine;
                }
                // PM map list to the caller.
                if (mapList.Count != 0)
                    userhandler.steamConnectionHandler.SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg, pmResponse);

                return chatResponse;




                return "Completed";
            }
        }

        private class Update : MapCommand
        {
            public Update(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!update", impnaomodule) { }
            protected override string exec(SteamID sender, string param)
            {

                return "Completed";
            }
        }

        private class Delete : MapCommand
        {
            ImpNaoModule impnaomodule;
            public Delete(VBot bot, ImpNaoModule module) : base(bot, "!impdelete", module)
            {
                impnaomodule = module;
            }
            protected override string exec(SteamID sender, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);
                impnaomodule.DeleteMapOnline(parameters[0]);
                return "Completed";
            }

        }

        private class Wipe : MapCommand
        {
            public Wipe(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!wipe", impnaomodule) { }
            protected override string exec(SteamID sender, string param)
            {

                return "Completed";
            }
        }
    }
}
