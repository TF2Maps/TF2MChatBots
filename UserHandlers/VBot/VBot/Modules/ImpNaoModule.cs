using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SteamBotLite
{
    class ImpNaoModule : BaseModule
    {
        public ObservableCollection<ImpNaoMap> MapListCache = new ObservableCollection<ImpNaoMap>();

        public ImpNaoModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            loadPersistentData();
            MapListCache.CollectionChanged += MapChange;
            
            commands.Add(new Add(bot, this));
            commands.Add(new Maps(bot, this));
            commands.Add(new Delete(bot, this));
            commands.Add(new UpdateName(bot, this));
            /*
            commands.Add(new Update(bot, this));
            adminCommands.Add(new Wipe(bot, this));
            */
        }
        public override void OnAllModulesLoaded()
        {

        }
        void MapChange(object sender, NotifyCollectionChangedEventArgs args)
        {
           // userhandler.OnMaplistchange(MapListCache, sender, args); //TODO Fix I guess?
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
            return JsonConvert.SerializeObject(MapListCache);
        }

        public override void loadPersistentData()
        {
            try
            {
                MapListCache = JsonConvert.DeserializeObject<ObservableCollection<ImpNaoMap>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                
            }
            catch
            {
                Console.WriteLine("Error Loading Map Cache List");
            }
        }

        public void HandleEvent(object sender, ServerModule.ServerInfo args)
        {
        }

        private sealed class UpdateName : BaseCommand
        {
            ImpNaoModule mapmodule;
            public UpdateName(VBot bot, ImpNaoModule module) : base(bot, "!nameupdate")
            {
                mapmodule = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
           //     userhandler.OnMaplistchange(mapmodule.MapListCache.Count, Msg, args); //TODO fix I guess?
                return "Name has been updated";
            }
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
            public bool uploadcheck(string filename, string Website)
            {
                return SearchClass.CheckDataExistsOnWebPage(Website, filename); //TODO develop method to check website
            }

            public Add(VBot bot, ImpNaoModule Impnaomodule) : base(bot, "!add", Impnaomodule) {
                impnaomodule = Impnaomodule;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);

                if (parameters.Length == 2)
                {
                    ImpNaoMap Map = new ImpNaoMap();
                    Map.name = parameters[0];
                    Map.link = parameters[1];
                    Map.password = Msg.ToString();
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

        public async Task DeleteMapOnline(string Map , ChatroomEntity user)
        {
            


            using (var client = new HttpClient())
            {
                
                var response = await client.DeleteAsync("http://carbidegames.com/impnao/api/maps/" + Map);

                var responseString = await response.Content.ReadAsStringAsync();
            }
        }

        private class Maps : MapCommand
        {
            ImpNaoModule impnaomodule;
            public Maps(VBot bot, ImpNaoModule module) : base(bot, "!maps", module)
            {
                impnaomodule = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string ImpNaoPage = SearchClass.GetWebPageAsString("http://carbidegames.com/impnao/api/maps");
                ObservableCollection<ImpNaoMap> mapList = JsonConvert.DeserializeObject<ObservableCollection<ImpNaoMap>>(ImpNaoPage);
                if (mapList.Count > 0)
                {
                    impnaomodule.MapListCache = mapList;
                    impnaomodule.savePersistentData();
                }
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
                    string mapLine = string.Format("{0} // {1} // {2} ({3})", mapList[i].name, mapList[i].link, mapList[i].Submitter, "Unknown");

                    if (!string.IsNullOrEmpty(mapList[i].Notes))
                        mapLine += "\nNotes: " + mapList[i].Notes;

                    if (i < mapList.Count - 1)
                        mapLine += "\n";

                    pmResponse += mapLine;
                }
                // PM map list to the caller.
                if (mapList.Count != 0)
                    userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = pmResponse });

                return chatResponse;


            }
        }

        private class MapCache : MapCommand
        {
            ImpNaoModule impnaomodule;
            public MapCache(VBot bot, ImpNaoModule module) : base(bot, "!mapscache", module)
            {
                impnaomodule = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                ObservableCollection< ImpNaoMap> mapList = impnaomodule.MapListCache;

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
                    string mapLine = string.Format("{0} // {1} // {2} ({3})", mapList[i].name, mapList[i].link, mapList[i].Submitter, "Unknown");

                    if (!string.IsNullOrEmpty(mapList[i].Notes))
                        mapLine += "\nNotes: " + mapList[i].Notes;

                    if (i < mapList.Count - 1)
                        mapLine += "\n";

                    pmResponse += mapLine;
                }
                // PM map list to the caller.
                if (mapList.Count != 0)
                    userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = pmResponse });

                return chatResponse;
            }
        }

        private class Update : MapCommand
        {
            public Update(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!update", impnaomodule) { }
            protected override string exec(MessageEventArgs Msg, string param)
            {

                return "Completed";
            }
        }

        private class Delete : MapCommand
        {
            ImpNaoModule impnaomodule;
            public Delete(VBot bot, ImpNaoModule module) : base(bot, "!delete", module)
            {
                impnaomodule = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string ImpNaoPage = SearchClass.GetWebPageAsString("http://carbidegames.com/impnao/api/maps");
                ObservableCollection<ImpNaoMap> mapList = JsonConvert.DeserializeObject<ObservableCollection<ImpNaoMap>>(ImpNaoPage);

                string[] parameters = param.Split(' ');

                if (parameters.Length > 0)
                {
                    ImpNaoMap deletedMap = mapList.FirstOrDefault(x => x.name == parameters[0]);

                    if (deletedMap == null)
                    {
                        return string.Format("Map '{0}' was not found.", parameters[0]);
                    }
                    else
                    {
                        if ((deletedMap.password.Equals(Msg.ToString())) || (userhandler.usersModule.admincheck(Msg.Sender)))
                        {
                            impnaomodule.DeleteMapOnline(parameters[0], Msg.Sender);
                            return string.Format("Map '{0}' DELETED.", deletedMap.name);
                        }
                        else
                        {
                            return string.Format("You do not have permission to edit map '{0}'.", deletedMap.name);
                        }
                    }
                }


                
              
                return "Completed";
            }

        }

        private class Wipe : MapCommand
        {
            public Wipe(VBot bot, ImpNaoModule impnaomodule) : base(bot, "!wipe", impnaomodule) { }
            protected override string exec(MessageEventArgs Msg, string param)
            {

                return "Completed";
            }
        }
    }
}
