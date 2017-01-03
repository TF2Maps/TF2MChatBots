using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Newtonsoft.Json;


namespace SteamBotLite
{
    class MapModule : BaseModule , ServerMapChangeListiner
    {
        // public List<Map> mapList = new List<Map>();  //OLD MAP SYSTEM
        public ObservableCollection<Map> mapList = new ObservableCollection<Map>();

        int MaxMapNumber = 10;
        string ServerMapListUrl;



        public MapModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {

            loadPersistentData();

            ServerMapListUrl = config["ServerMapListUrl"].ToString();
            MaxMapNumber = int.Parse(config["MaxMapList"].ToString());
            Console.WriteLine("URL list is now {0} and maximum map number {1}", ServerMapListUrl, MaxMapNumber);

            userhandler = bot;

            //WebServer = new MapWebServer("http://localhost:8080/index/",this);

            mapList.CollectionChanged += MapChange;
            ConvertMaplistToTable();

            commands.Add(new Add(bot, this));
            commands.Add(new Maps(bot, this));
            commands.Add(new Update(bot, this));
            commands.Add(new UpdateName(bot, this));
            commands.Add(new Delete(bot, this));
            commands.Add(new UploadCheck(bot, ServerMapListUrl));
            adminCommands.Add(new Insert(bot, this));
            adminCommands.Add(new Reposition(bot, this));
            adminCommands.Add(new Wipe(bot, this));
        }

        void MapChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            userhandler.OnMaplistchange(mapList, sender, args);
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

        public class Map
        {
            public Object Submitter { get; set; }
            public string SubmitterName { get; set; }

            public string DownloadURL { get; set; }
            public string Notes { get; set; }

            int MaxCharacters = 27;
            private string filename;



            public string Filename
            {
                get
                {
                    return filename;
                }
                set
                {
                    if (value.Length > MaxCharacters)
                    {
                        throw new ArgumentException("It includes too many characters: " + MaxCharacters);
                    }

                    if (value.Any(c => char.IsUpper(c)))
                    {
                        throw new ArgumentException("It includes an uppercase letter");
                    }

                    filename = value;
                }
            }
        }



        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(mapList);
        }

        public override void loadPersistentData()
        {
            try
            {
                Console.WriteLine("Loading Map List");
                mapList = JsonConvert.DeserializeObject<ObservableCollection<Map>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                Console.WriteLine("Loaded Map List");
            }
            catch
            {
                mapList = new ObservableCollection<Map>();
                Console.WriteLine("Error Loading Map List, creating a new one and wiping the old");
            }
        }

        public void OnMapChange(ServerModule.ServerInfo args)
        {
            Console.WriteLine("Going to possibly remove {0} Map...", args.currentMap);
            Map map = mapList.FirstOrDefault(x => x.Filename == args.currentMap);


            if (map != null)
            {
                ChatroomEntity Submitter = new ChatroomEntity(map.Submitter,ChatroomEntity.Individual.User,null);
                Console.WriteLine("Found map, sending message to {0}", Submitter);
                userhandler.SendPrivateMessageProcessEvent(new MessageProcessEventData(null) { Sender = Submitter, ReplyMessage = string.Format("Map {0} is being tested on the {1} server and has been DELETED.", map.Filename, args.tag) });
                mapList.Remove(map);
                Console.WriteLine("Map {0} is being tested on the {1} server and has been DELETED.", map.Filename, args.tag);
                savePersistentData();
            }
            Console.Write("...Not Found");
            return;
        }

        // The abstract command for motd

        abstract public class MapCommand : BaseCommand
        {
            protected MapModule MapModule;

            public MapCommand(VBot bot, string command, MapModule mapMod)
                : base(bot, command)
            {
                this.MapModule = mapMod;
            }
        }

        private sealed class UploadCheck : BaseCommand
        {
            string ServerMapListURL;
            public UploadCheck(VBot bot, string Website) : base(bot, "!uploadcheck")
            {
                ServerMapListURL = Website;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                return SearchClass.CheckDataExistsOnWebPage(ServerMapListURL, param).ToString();
            }
        }

        private sealed class UpdateName : BaseCommand
        {
            MapModule mapmodule;
            public UpdateName(VBot bot, MapModule module) : base(bot, "!nameupdate")
            {
                mapmodule = module;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
                userhandler.OnMaplistchange(mapmodule.mapList, Msg, args);
                return "Name has been updated";
            }
        }

        // The commands

        private class Add : MapCommand
        {

            public bool uploadcheck(string filename, string Website)
            {
                return SearchClass.CheckDataExistsOnWebPage(Website, filename); //TODO develop method to check website
            }

            public Add(VBot bot, MapModule mapModule) : base(bot, "!add", mapModule) { }

            protected override string exec(MessageProcessEventData Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);

                Map map = new Map();
                map.Submitter = Msg.Sender.identifier.ToString();

                map.SubmitterName = Msg.Sender.DisplayName;

                try
                {
                    map.Filename = parameters[0];
                }
                catch (Exception exception)
                {
                    return string.Format("Your map is rejected because: {0}", exception.Message);
                }

                map.Filename = parameters[0];
                map.Notes = "";


                if (parameters[0].Length == 0)
                {
                    return "Invalid parameters for !add. Syntax: !add <filename> <url> <notes>";
                }

                if (uploadcheck(map.Filename, MapModule.ServerMapListUrl)) //Check if the map is uploaded
                {
                    map.DownloadURL = "Uploaded";
                    if (parameters.Length > 1)
                    {
                        map.Notes = parameters.Last();
                    }
                }
                else if (parameters.Length > 1) //If its not uploaded check if a URL was there
                {
                    parameters = param.Split(new char[] { ' ' }, 3);

                    map.DownloadURL = parameters[1];

                    if (parameters.Length > 2)
                    {
                        map.Notes = parameters.Last();
                    }
                }
                else
                {
                    return "Your map isn't uploaded! Please use include the url with the syntax: !add <filename> <url> (notes)";
                }

                if (MapModule.mapList.Any(m => m.Filename.Equals(map.Filename)))
                {
                    return "Your map has been rejected as it already exists in the map list!";
                }

                else

                {
                    MapModule.mapList.Add(map);
                    MapModule.savePersistentData();

                    string Reply = string.Format("'{0}' is now on the list.", map.Filename);
                    return Reply;
                }
            }
        }

        private class Insert : MapCommand
        {

            public bool uploadcheck(string filename, string Website)
            {
                return SearchClass.CheckDataExistsOnWebPage(Website, filename); //TODO develop method to check website
            }

            public Insert(VBot bot, MapModule mapModule) : base(bot, "!insert", mapModule) { }

            protected override string exec(MessageProcessEventData msg, string param)
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

                if (uploadcheck(map.Filename, MapModule.ServerMapListUrl)) //Check if the map is uploaded
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

                MapModule.mapList.Insert(index, map);

                MapModule.savePersistentData();

                return Reply;

            }
        }

        private class Maps : MapCommand
        {
            public Maps(VBot bot, MapModule mapMod) : base(bot, "!maps", mapMod) { }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                var maps = MapModule.mapList;
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
                    userhandler.SendPrivateMessageProcessEvent(new MessageProcessEventData(null) { Sender = Msg.Sender, ReplyMessage = pmResponse });
                }

                return chatResponse;
            }
        }

        private class Reposition : MapCommand
        {
            public Reposition(VBot bot, MapModule mapMod) : base(bot, "!reposition", mapMod) { }
            protected override string exec(MessageProcessEventData Msg, string param)
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
                    if (editedMap.Submitter.Equals(Msg.Sender.identifier.ToString()) | (userhandler.usersModule.admincheck(Msg.Sender)))
                    {
                        MapModule.mapList.Remove(editedMap);
                        editedMap.Notes += string.Format("Map repositioned to {0} by {1} // ", index, Msg.Sender.identifier.ToString());
                        MapModule.mapList.Insert(index, editedMap);
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
            public Update(VBot bot, MapModule mapMod) : base(bot, "!update", mapMod) { }
            protected override string exec(MessageProcessEventData msg, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length < 2)
                {
                    return string.Format("Invalid parameters for !update. Syntax: !update <Current filename> <New filename> <New Url> <New Notes>");
                }
                else
                {
                    int Index = 0;
                    bool MapExists = false;
                    foreach (Map Entry in MapModule.mapList)
                    {
                        if (Entry.Filename.Equals(parameters[0]) && (Entry.Submitter.ToString().Equals(msg.Sender.identifier.ToString()) | (userhandler.usersModule.admincheck(msg.Sender))))
                        {
                            MapExists = true;
                            break;
                        }
                        else
                        {
                            Index++;
                        }
                    }
                    if (MapExists)
                    {
                        MapModule.mapList[Index].Filename = parameters[1];
                        if (parameters.Length > 2)
                        {
                            MapModule.mapList[Index].DownloadURL = parameters[2];
                        }
                        MapModule.savePersistentData();
                        
                        return string.Format("Map renamed to'{0}'", MapModule.mapList[Index].Filename);
                    }
                    else
                    {
                        return string.Format("The map was not found or you don't have permission to edit it!");
                    }
                }
            }
        }

        private class Delete : MapCommand
        {
            public Delete(VBot bot, MapModule mapMod) : base(bot, "!delete", mapMod) { }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length > 0)
                {
                    Map deletedMap = MapModule.mapList.FirstOrDefault(x => x.Filename == parameters[0]);

                    if (deletedMap == null)
                    {
                        return string.Format("Map '{0}' was not found.", parameters[0]);
                    }
                    else
                    {
                        if ((deletedMap.Submitter.Equals(Msg.Sender.identifier.ToString())) || (userhandler.usersModule.admincheck(Msg.Sender)))
                        {
                            MapModule.mapList.Remove(deletedMap);
                            MapModule.savePersistentData();
                            userhandler.SendPrivateMessageProcessEvent(new MessageProcessEventData(null) { Sender = new ChatroomEntity(deletedMap.Submitter,ChatroomEntity.Individual.User,null), ReplyMessage = string.Format("Your map {0} has been deleted from the map list", deletedMap.Filename) });
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
            public Wipe(VBot bot, MapModule mapMod) : base(bot, "!wipe", mapMod)
            {
                module = mapMod;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
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


        void ClearMapListWithMessage(string message)
        {
            ObservableCollection<Map> TempMapList = new ObservableCollection<Map>();

            while (mapList.Count > 0)
            {
                MessageProcessEventData Msg = new MessageProcessEventData(null);
                Msg.ReplyMessage = string.Format("Hi, the Maplist has been cleared and your map was removed for the following reason: {0}", message);
                ChatroomEntity sender = new ChatroomEntity(mapList[0].Submitter, ChatroomEntity.Individual.User, null);
                userhandler.SendPrivateMessageProcessEvent(Msg);
                mapList.RemoveAt(0);
            };
        }
    }
}
        /*
        private class WebServerStart : BaseCommand
        {
            protected MapModule MapModule;
            

            public WebServerStart(VBot bot, string command , MapModule module)
                : base(bot, command)
            {
                
                MapModule = module;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                MapModule.WebServer = new MapWebServer(param, MapModule.mapList);
                MapModule.mapList.CollectionChanged += MapModule.WebServer.MapListUpdate;
                if (MapModule.WebServer != null)
                {
                    return "Started the Web Server";
                }
                else
                {
                    return "An error prevented the webserver from loading";
                }
            }
        }
        private class WebServerStop : BaseCommand
        {
            protected MapModule MapModule;
            VBot bot;

            public WebServerStop(VBot bot, string command, MapModule module)
                : base(bot, command)
            {
                MapModule = module;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                MapModule.mapList.CollectionChanged -= MapModule.WebServer.MapListUpdate;
                MapModule.WebServer = null;
                
                return "Faded the Web Server away and made it OBSOLETE";
            }
        }
        */
   

