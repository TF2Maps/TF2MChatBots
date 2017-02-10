using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Newtonsoft.Json;


namespace SteamBotLite
{
    public class MapModule : BaseModule , ServerMapChangeListiner
    {
        // public List<Map> mapList = new List<Map>();  //OLD MAP SYSTEM

        public MapCollection mapList;

        int MaxMapNumber = 10;
        string ServerMapListUrl;
        
        public MapModule(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            LoadModule(bot);

        }
        public MapModule(ModuleHandler bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            LoadModule(bot);
        }

        void LoadModule (ModuleHandler bot)
        {
            loadPersistentData();

            ServerMapListUrl = config["ServerMapListUrl"].ToString();
            MaxMapNumber = int.Parse(config["MaxMapList"].ToString());

            Console.WriteLine("URL list is now {0} and maximum map number {1}", ServerMapListUrl, MaxMapNumber);

            userhandler = bot;
            mapList.CollectionChanged += MapChange;

            ConvertMaplistToTable();
            commands.Add(new Add(bot, this));
            commands.Add(new Maps(bot, this));
            commands.Add(new Update(bot, this));
            commands.Add(new UpdateName(bot, this));
            commands.Add(new Delete(bot, this));
            commands.Add(new UploadCheck(bot, this));
            adminCommands.Add(new Insert(bot, this));
            adminCommands.Add(new Reposition(bot, this));
            adminCommands.Add(new Wipe(bot, this));
            adminCommands.Add(new AllowOnlyUploadedMapsSetter(bot, this));

            bot.AddMapChangeEventListiner(this);
        }

        bool CheckIfMapIsUploaded(string filename)
        {
            if (SubstitutingWebPage)
            {
                return (MapListUploadCheck.Contains(filename));
            }
            else
            {
                return SearchClass.CheckDataExistsOnWebPage(ServerMapListUrl, filename);
            }
        }


        //This exists for testing purposes, Allowing us to emulate a webpage being returned

        bool SubstitutingWebPage = false;
        string MapListUploadCheck; 

        public void SubstituteWebPageWithString (string data)
        {
            SubstitutingWebPage = true;
            MapListUploadCheck = data;
        }

        public override void OnAllModulesLoaded()
        {
            userhandler.OnMaplistchange(mapList.GetAllMaps(), null, null); 
        }

        void MapChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            userhandler.OnMaplistchange(mapList.GetAllMaps(), sender, args);
            ConvertMaplistToTable();
        }

        void ConvertMaplistToTable ()
        {
            string[] HeaderNames = new string[] { "Filename", "Url", "Notes", "Submitter" };
            List<string[]> DataEntries = new List<string[]>();

            foreach(Map entry in mapList)
            {
                string[] Data = new string[] { entry.Filename, entry.DownloadURL, entry.Notes, entry.SubmitterName };
                DataEntries.Add(Data);
            }
            userhandler.HTMLFileFromArray(HeaderNames, DataEntries, "CurrentMaps");
        }

        public override string getPersistentData()
        {
            Dictionary<string, string> PersistantData = new Dictionary<string, string>();
            PersistantData.Add("Maplist", JsonConvert.SerializeObject(mapList.GetAllMaps()));

            Dictionary<string, object> ConfigData = new Dictionary<string, object>();
            ConfigData.Add("AllowOnlyUploadedMaps", mapList.AllowOnlyUploadedMaps.ToString());
            ConfigData.Add("AllowOnlyUploadedMapsErrMsg",mapList.ForceMapsToBeUploadedErrorResponse);

            PersistantData.Add("Config", JsonConvert.SerializeObject(ConfigData));

            return JsonConvert.SerializeObject(PersistantData);
        }

        public override void loadPersistentData()
        {
            try
            {
                Console.WriteLine("Loading Map List");
               

                Dictionary<string, string> PersistantData = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                mapList = new MapCollection(JsonConvert.DeserializeObject<ObservableCollection<Map>>(PersistantData["Maplist"].ToString()));

                Dictionary<string, object> MetaData = new Dictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(PersistantData["Config"]));
                bool AllowOnlyUploadedMaps = (bool.Parse(MetaData["AllowOnlyUploadedMaps"].ToString()));
                string AllowOnlyUploadedMapsErrMsg = MetaData["AllowOnlyUploadedMapsErrMsg"].ToString();
                mapList.RestrictMapsToBeUploaded(AllowOnlyUploadedMaps, AllowOnlyUploadedMapsErrMsg);

                Console.WriteLine("Loaded Map List");
            }
            catch
            {
                mapList = new MapCollection(new ObservableCollection<Map>());
                mapList.RestrictMapsToBeUploaded(false,"TESTING");
                
                savePersistentData();

                Console.WriteLine("Error Loading Map List, creating a new one and wiping the old");
            }
        }

        public void OnMapChange(ServerInfo args)
        {
            Console.WriteLine("Going to possibly remove {0} Map...", args.currentMap);

            Map map = mapList.GetMapByFilename(args.currentMap);

            if (map != null && args.playerCount > 8)
            {
                User Submitter = new User(map.Submitter,null);

                Console.WriteLine("Found map, sending message to {0}", Submitter);
                userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Submitter, ReplyMessage = string.Format("Map {0} is being tested on the {1} server and has been DELETED.", map.Filename, args.tag) });
                mapList.RemoveMap(map);
                Console.WriteLine("Map {0} is being tested on the {1} server and has been DELETED.", map.Filename, args.tag);
                savePersistentData();
            }
            Console.Write("...Not Found");
            return;
        }
        

        abstract public class MapCommand : BaseCommand
        {
            protected MapModule MapModule;

            public MapCommand(ModuleHandler bot, string command, MapModule mapMod)
                : base(bot, command)
            {
                this.MapModule = mapMod;
            }

            public Map CleanupAndParseMsgToMap (MessageEventArgs Msg , string param)
            {
                string message = RemoveWhiteSpacesFromString(param);

                Map map = ParseStringToMap(message);
                map.Submitter = Msg.Sender.identifier;
                map.SubmitterName = Msg.Sender.DisplayName;

                return map;
            }

            public Map ParseStringToMap(string message)
            {
                string[] parameters = message.Split(new char[] { ' ' }, 2);
                string trailer = "";

                if (parameters.Length > 1) {
                    trailer = parameters[1];
                }

                Map map = new Map();
                map.Filename = parameters[0];

                if (MapIsUploadedToWebsite(map.Filename)){
                    map.Uploaded = true;
                }
                else {
                    string[] TrailerSplitByFirstWord = trailer.Split(new char[] { ' ' }, 2);

                    map.DownloadURL = TrailerSplitByFirstWord[0];

                    if (TrailerSplitByFirstWord.Length > 1) {
                        trailer = TrailerSplitByFirstWord[1];
                    }
                }

                map.Notes = trailer;

                return map; 
            }

            public bool MapIsUploadedToWebsite(string filename)
            {
                return MapModule.CheckIfMapIsUploaded(filename);
            }
        }

        private sealed class UploadCheck : MapCommand
        {
            public UploadCheck(ModuleHandler bot, MapModule mapModule) : base(bot, "!uploadcheck", mapModule)
            {}

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return MapModule.CheckIfMapIsUploaded(param).ToString();
            }
        }

        private sealed class AllowOnlyUploadedMapsSetter : BaseCommand
        {
            MapModule mapmodule;
            public AllowOnlyUploadedMapsSetter(ModuleHandler bot, MapModule module) : base(bot, "!forceuploaded")
            {
                mapmodule = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);
                bool AllowOnlyUploadedMaps = bool.Parse(parameters[0]);
                string RejectUnUploadedMapsReply = parameters[1];
                mapmodule.mapList.RestrictMapsToBeUploaded(AllowOnlyUploadedMaps, RejectUnUploadedMapsReply);

                mapmodule.savePersistentData();

                return string.Format("Config has been updated, forcing maps to be uploaded has been set to: {0} with an error msg {1}",mapmodule.mapList.AllowOnlyUploadedMaps.ToString(), mapmodule.mapList.ForceMapsToBeUploadedErrorResponse);
            }
        }

        private sealed class UpdateName : BaseCommand
        {
            MapModule mapmodule;
            public UpdateName(ModuleHandler bot, MapModule module) : base(bot, "!nameupdate")
            {
                mapmodule = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                userhandler.OnMaplistchange(mapmodule.mapList.GetAllMaps(), Msg, args);
                return "Name has been updated";
            }
        }

        // The commands

        private class Add : MapCommand
        {

            
            public Add(ModuleHandler bot, MapModule mapModule) : base(bot, "!add", mapModule) { }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                Map UserMap = CleanupAndParseMsgToMap(Msg , param);
                UserMap.Submitter = Msg.Sender.identifier;

                string Reply =  MapModule.mapList.AddMap(UserMap);

                MapModule.savePersistentData();

                return Reply;

            }
        }

        private class Insert : MapCommand
        {

            

            public Insert(ModuleHandler bot, MapModule mapModule) : base(bot, "!insert", mapModule) { }

            protected override string exec(MessageEventArgs msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 3);
                int index;

                if (parameters[0].Length == 0)
                {
                    return "Invalid parameters for !insert. Syntax: !insert <index> <filename> <url> <notes>";
                }
                try
                {
                    index = int.Parse(parameters[0]);
                }
                catch
                {
                    return "Invalid parameters for !insert. Syntax: !insert <index> <filename> <url> <notes>";
                }

                Map map = new Map();
                map.Submitter = msg.Sender.identifier.ToString();

                map.SubmitterName = msg.Sender.DisplayName;

                try
                {
                    map.Filename = parameters[1];
                }
                catch (Exception exception)
                {
                    return string.Format("Your new file name was rejected because: {0}", exception.Message);
                }


                map.Notes = string.Format("Inserted in position {0} by {1} //", index, msg.Sender.identifier.ToString());

                if (MapModule.CheckIfMapIsUploaded(map.Filename)) //Check if the map is uploaded
                {
                    map.DownloadURL = "Uploaded";
                    if (parameters.Length > 1)
                    {
                        map.Notes += parameters.Last();
                    }
                }
                else if (parameters.Length > 2) //If its not uploaded check if a URL was there
                {
                    parameters = param.Split(new char[] { ' ' }, 4);

                    map.DownloadURL = parameters[2];
                    if (parameters.Length > 3)
                    {
                        map.Notes += parameters.Last();
                    }
                }
                else //If a url isn't there lets return an error
                {
                    return "Your map isn't uploaded! Please use include the url with the syntax: !add <filename> <url> (notes)";
                }
                string Reply = string.Format("Map '{0}' added.", map.Filename);

                MapModule.mapList.InsertMap(index, map);

                MapModule.savePersistentData();

                return Reply;

            }
        }

        private class Maps : MapCommand
        {
            public Maps(ModuleHandler bot, MapModule mapMod) : base(bot, "!maps", mapMod) { }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                var maps = MapModule.mapList.GetAllMaps();
                int maxMaps = MapModule.MaxMapNumber;
                string chatResponse = "";
                string pmResponse = "";

                // Take the max number of maps.
                var mapList = maps
                    .Take(maxMaps)
                    .ToList();

                if (maps.Count == 0)
                {
                    chatResponse = "The map list is empty.";
                }
                else
                {
                    // Build the chat response.
                    chatResponse = string.Join(" , ", mapList.Select(x => x.Filename));
                    if (maps.Count > maxMaps)
                    {
                        chatResponse += string.Format(" (and {0} more at: http://vbot.site )", maps.Count - maxMaps);
                    }
                    else
                    {
                        chatResponse += " at: http://vbot.site";
                    }

                    // Build the private response.
                    pmResponse = "";
                    for (int i = 0; i < maps.Count; i++)
                    {
                        string mapLine = string.Format("{0} // {1} // {2} ({3})", maps[i].Filename, maps[i].DownloadURL, maps[i].SubmitterName, maps[i].Submitter.ToString());

                        if (!string.IsNullOrEmpty(maps[i].Notes))
                            mapLine += "\nNotes: " + maps[i].Notes;

                        if (i < maps.Count - 1)
                            mapLine += "\n";

                        pmResponse += mapLine;
                    }
                }

                // PM map list to the caller.
                if (maps.Count != 0)
                {
                    userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = pmResponse });
                }

                return chatResponse;
            }
        }

        private class Reposition : MapCommand
        {
            public Reposition(ModuleHandler bot, MapModule mapMod) : base(bot, "!reposition", mapMod) { }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length < 2)
                {
                    return string.Format("Invalid parameters for !reposition. Syntax: !reposition <new position> <filename>");
                }
                else
                {
                    int index;
                    try
                    {
                        index = int.Parse(parameters[0]);
                    }
                    catch
                    {
                        return string.Format("Invalid parameters for !reposition. Syntax: !reposition <new position> <filename>");
                    }
                    Map editedMap = null;
                    foreach (Map entry in MapModule.mapList)
                    {
                        if (entry.Filename == parameters[1])
                            editedMap = entry;
                    }

                    if (editedMap == null)
                    {
                        return "Map not found";
                    }


                    // Map editedMap = MapModule.mapList.Find(map => map.filename.Equals(parameters[0])); //OLD Map CODE
                    if (editedMap.Submitter.Equals(Msg.Sender.identifier.ToString()) | (userhandler.admincheck(Msg.Sender)))
                    {
                        MapModule.mapList.RemoveMap(editedMap);
                        editedMap.Notes += string.Format("Map repositioned to {0} by {1} // ", index, Msg.Sender.identifier.ToString());
                        MapModule.mapList.InsertMap(index, editedMap);
                        MapModule.savePersistentData();
                        return string.Format("Map '{0}' has been repositioned to {1}.", editedMap.Filename, index);
                    }
                    else
                    {
                        return string.Format("You cannot edit map '{0}' as you did not submit it.", editedMap.Filename);
                    }
                }
            }
        }


        private class Update : MapCommand
        {
            public Update(ModuleHandler bot, MapModule mapMod) : base(bot, "!update", mapMod) { }
            protected override string exec(MessageEventArgs msg, string param)
            {
                //string[] parameters = param.Split(' ');
                string[] parameters = param.Split(new char[] { ' ' }, 2);

                if (parameters.Length > 1)
                {
                    Map NewMapdata = ParseStringToMap(parameters[1]);
                    return MapModule.mapList.UpdateMap(parameters[0], NewMapdata, msg.Sender);
                }
                else
                {
                    return string.Format("Invalid parameters for !update. Syntax: !update <Current filename> <New filename> <New Url> <New Notes>");
                }
            }
        }

        private class Delete : MapCommand
        {
            public Delete(ModuleHandler bot, MapModule mapMod) : base(bot, "!delete", mapMod) { }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length > 0)
                {
                    Map deletedMap = MapModule.mapList.GetMapByFilename(parameters[0]);

                    if (deletedMap == null)
                    {
                        return string.Format("Map '{0}' was not found.", parameters[0]);
                    }
                    else
                    {
                        if ((deletedMap.Submitter.Equals(Msg.Sender.identifier.ToString())) || (userhandler.admincheck(Msg.Sender)))
                        {
                            MapModule.mapList.RemoveMap(deletedMap);
                            MapModule.savePersistentData();
                            userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = new User(deletedMap.Submitter,null), ReplyMessage = string.Format("Your map {0} has been deleted from the map list", deletedMap.Filename) });
                            return string.Format("Map '{0}' DELETED.", deletedMap.Filename);
                        }
                        else
                        {
                            return string.Format("You do not have permission to edit map '{0}'.", deletedMap.Filename);
                        }
                    }
                }
                return "Invalid parameters for !delete. Syntax: !delete <filename>";
            }

        }

        private class Wipe : MapCommand
        {
            MapModule module;
            public Wipe(ModuleHandler bot, MapModule mapMod) : base(bot, "!wipe", mapMod)
            {
                module = mapMod;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                if (!string.IsNullOrEmpty(param))
                {
                    module.ClearMapListWithMessage(param);
                    MapModule.savePersistentData();
                    return "The map list has been DELETED.";
                }
                else
                {
                    return "The map list has not been DELETED, you must include a reason! !wipe <reason>";
                }
            }
        }


        public void ClearMapListWithMessage(string message)
        {
            ObservableCollection<Map> TempMapList = new ObservableCollection<Map>();

            while (mapList.GetSize() > 0)
            {
                MessageEventArgs Msg = new MessageEventArgs(null);
                Msg.ReplyMessage = string.Format("Hi, the Maplist has been cleared and your map was removed for the following reason: {0}", message);
                Msg.Destination = new User(mapList.GetMap(0).Submitter,  null);
                userhandler.SendPrivateMessageProcessEvent(Msg);
                mapList.RemoveMap(0);
            };
            savePersistentData();
        }
    }
}