using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;

namespace SteamBotLite
{
    public class MapModule : BaseModule, ServerMapChangeListiner
    {
        // public List<Map> mapList = new List<Map>();  //OLD MAP SYSTEM

        public MapCollection mapList;

        private int MaxMapNumber = 10;
        private string ServerMapListUrl;

        public MapModule(VBot bot, HTMLFileFromArrayListiners HtmlListiner, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            this.HTMLlistiner = HtmlListiner;
            LoadModule(bot);
        }

        private HTMLFileFromArrayListiners HTMLlistiner;

        public MapModule(ModuleHandler bot, HTMLFileFromArrayListiners HtmlListiner, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            this.HTMLlistiner = HtmlListiner;
            LoadModule(bot);
        }

        private void LoadModule(ModuleHandler bot)
        {
            HTMLlistiner.SetTableHeader(TableName, GetMapListTableHeader()); //Ensures the maplist is shown before deleted maps

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
            commands.Add(new GetOwner(bot, this));

            bot.AddMapChangeEventListiner(this);
        }

        private bool CheckIfMapIsUploaded(string filename)
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

        private bool SubstitutingWebPage = false;
        private string MapListUploadCheck;

        public void SubstituteWebPageWithString(string data)
        {
            SubstitutingWebPage = true;
            MapListUploadCheck = data;
        }

        public override void OnAllModulesLoaded()
        {
            userhandler.OnMaplistchange(mapList.GetAllMaps(), null, null);
        }

        private void MapChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            userhandler.OnMaplistchange(mapList.GetAllMaps(), sender, args);
            ConvertMaplistToTable();
        }

        private string TableName = "Current Maps";

        private TableDataValue[] GetMapListTableHeader()
        {
            TableDataValue[] HeaderName = new TableDataValue[4];
            HeaderName[0] = new TableDataValue(); //There's gotta be a way to fix this
            HeaderName[1] = new TableDataValue(); //Too long, too useless
            HeaderName[2] = new TableDataValue();
            HeaderName[3] = new TableDataValue();

            HeaderName[0].VisibleValue = "Filename";
            HeaderName[1].VisibleValue = "Url";
            HeaderName[2].VisibleValue = "Notes";
            HeaderName[3].VisibleValue = "Submitter";

            return HeaderName;
        }

        private void ConvertMaplistToTable()
        {
            List<TableDataValue[]> Entries = new List<TableDataValue[]>();

            foreach (Map entry in mapList.GetAllMaps())
            {
                TableDataValue[] Values = new TableDataValue[4];
                Values[0] = new TableDataValue();
                Values[1] = new TableDataValue();
                Values[2] = new TableDataValue();
                Values[3] = new TableDataValue();

                Values[0].VisibleValue = entry.Filename;

                Values[1].VisibleValue = entry.DownloadURL;
                Values[1].Link = entry.DownloadURL;

                Values[2].VisibleValue = entry.Notes;

                Values[3].VisibleValue = entry.SubmitterName;
                Values[3].HoverText = entry.Submitter.ToString();

                if (string.IsNullOrEmpty(entry.SubmitterContact))
                {
                }
                else
                {
                    Values[3].Link = entry.SubmitterContact;
                }

                Entries.Add(Values);

                HTMLlistiner.AddEntryWithoutLimit(TableName, Values);
            }

            TableData data = new TableData();
            data.Header = GetMapListTableHeader();
            data.TableValues = Entries;

            HTMLlistiner.MakeTableFromEntry(TableName, data);
        }

        public override string getPersistentData()
        {
            Dictionary<string, string> PersistantData = new Dictionary<string, string>();
            PersistantData.Add("Maplist", JsonConvert.SerializeObject(mapList.GetAllMaps()));

            Dictionary<string, object> ConfigData = new Dictionary<string, object>();
            ConfigData.Add("AllowOnlyUploadedMaps", mapList.AllowOnlyUploadedMaps.ToString());
            ConfigData.Add("AllowOnlyUploadedMapsErrMsg", mapList.ForceMapsToBeUploadedErrorResponse);

            PersistantData.Add("Config", JsonConvert.SerializeObject(ConfigData));

            return JsonConvert.SerializeObject(PersistantData);
        }

        public override void loadPersistentData()
        {
            try
            {
                Console.WriteLine("Loading Map List");

                Dictionary<string, string> PersistantData = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                mapList = new MapCollection(JsonConvert.DeserializeObject<ObservableCollection<Map>>(PersistantData["Maplist"].ToString()), HTMLlistiner);

                Dictionary<string, object> MetaData = new Dictionary<string, object>(JsonConvert.DeserializeObject<Dictionary<string, object>>(PersistantData["Config"]));
                bool AllowOnlyUploadedMaps = (bool.Parse(MetaData["AllowOnlyUploadedMaps"].ToString()));
                string AllowOnlyUploadedMapsErrMsg = MetaData["AllowOnlyUploadedMapsErrMsg"].ToString();
                mapList.RestrictMapsToBeUploaded(AllowOnlyUploadedMaps, AllowOnlyUploadedMapsErrMsg);

                Console.WriteLine("Loaded Map List");
            }
            catch
            {
                mapList = new MapCollection(new ObservableCollection<Map>(), HTMLlistiner);
                mapList.RestrictMapsToBeUploaded(false, "TESTING");

                savePersistentData();

                Console.WriteLine("Error Loading Map List, creating a new one and wiping the old");
            }
        }

        public void OnMapChange(TrackingServerInfo args)
        {
            Console.WriteLine("Going to possibly remove {0} Map...", args.currentMap);

            Map map = mapList.GetMapByFilename(args.currentMap);

            if (map != null && args.playerCount > 8)
            {
                User Submitter = new User(map.Submitter, null);

                Console.WriteLine("Found map, sending message to {0}", Submitter);
                string ReasonForDeletion = string.Format("Map {0} is being tested on the {1} server and has been DELETED.", map.Filename, args.tag);
                userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Submitter, ReplyMessage = ReasonForDeletion });
                mapList.RemoveMap(map, ReasonForDeletion);
                Console.WriteLine("Map {0} is being tested on the {1} server and has been DELETED.", map.Filename, args.tag);
                savePersistentData();
            }
            Console.Write("...Not Found");
            return;
        }

        abstract public class MapCommand : BaseCommand
        {
            protected MapModule MapModule;
            private string syntax;
            private string command;

            public MapCommand(ModuleHandler bot, string command, MapModule mapMod, string Syntax) : base(bot, command)
            {
                this.MapModule = mapMod;
                this.syntax = Syntax;
                this.command = command;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] commandsplit = param.Split(new char[] { ' ' }, 2);

                if ((commandsplit[0].StartsWith(command, StringComparison.OrdinalIgnoreCase)))
                {
                    return "Incorrect Syntax!: " + syntax;
                }
                else
                {
                    return runcommand(Msg, param);
                }
            }

            public abstract string runcommand(MessageEventArgs Msg, string param);

            public Map CleanupAndParseMsgToMap(MessageEventArgs Msg, string param)
            {
                string message = RemoveWhiteSpacesFromString(param);

                Map map = ParseStringToMap(message);
                map.Submitter = Msg.Sender.identifier;
                map.SubmitterName = Msg.Sender.DisplayName;
                map.SubmitterContact = Msg.Sender.UserURL;

                return map;
            }

            public Map ParseStringToMap(string message)
            {
                string[] parameters = message.Split(new char[] { ' ' }, 2);
                string trailer = "";

                if (parameters.Length > 1)
                {
                    trailer = parameters[1];
                }

                Map map = new Map();
                map.Filename = parameters[0];

                if (MapIsUploadedToWebsite(map.Filename))
                {
                    map.Uploaded = true;
                }
                else
                {
                    string[] TrailerSplitByFirstWord = trailer.Split(new char[] { ' ' }, 2);

                    map.DownloadURL = TrailerSplitByFirstWord[0];

                    if (TrailerSplitByFirstWord.Length > 1)
                    {
                        trailer = TrailerSplitByFirstWord[1];
                    }
                    else
                    {
                        trailer = "";
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
            public UploadCheck(ModuleHandler bot, MapModule mapModule) : base(bot, "!uploadcheck", mapModule, "!uploadcheck <mapname>")
            { }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                return MapModule.CheckIfMapIsUploaded(param).ToString();
            }
        }

        private sealed class AllowOnlyUploadedMapsSetter : BaseCommand
        {
            private MapModule mapmodule;

            public AllowOnlyUploadedMapsSetter(ModuleHandler bot, MapModule module) : base(bot, "!forceuploaded")
            {
                mapmodule = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);
                bool AllowOnlyUploadedMaps = bool.Parse(parameters[0]);
                if (parameters.Count() != 2 && AllowOnlyUploadedMaps)
                {
                    return "The syntax is: !forceuploaded true/false <reason>";
                }
                if (AllowOnlyUploadedMaps == false)
                {
                    mapmodule.mapList.RestrictMapsToBeUploaded(AllowOnlyUploadedMaps, "");
                }
                else
                {
                    string RejectUnUploadedMapsReply = parameters[1];
                    mapmodule.mapList.RestrictMapsToBeUploaded(AllowOnlyUploadedMaps, RejectUnUploadedMapsReply);
                }
                mapmodule.savePersistentData();

                return string.Format("Config has been updated, forcing maps to be uploaded has been set to: {0} with an error msg: {1}", mapmodule.mapList.AllowOnlyUploadedMaps.ToString(), mapmodule.mapList.ForceMapsToBeUploadedErrorResponse);
            }
        }

        private sealed class UpdateName : BaseCommand
        {
            private MapModule mapmodule;

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
            public Add(ModuleHandler bot, MapModule mapModule) : base(bot, "!add", mapModule, "!add <mapname> <url> <notes>")
            {
            }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                Map UserMap = CleanupAndParseMsgToMap(Msg, param);

                string Reply = MapModule.mapList.AddMap(UserMap);
                MapModule.savePersistentData();

                return Reply;
            }
        }

        private class Insert : MapCommand
        {
            public Insert(ModuleHandler bot, MapModule mapModule) : base(bot, "!insert", mapModule, "!insert <index> <filename> <url> <notes>")
            {
            }

            public override string runcommand(MessageEventArgs msg, string param)
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

        private class Maps : BaseCommand
        {
            private MapModule module;

            private DateTime LastExecuted;

            public Maps(ModuleHandler bot, MapModule mapMod) : base(bot, "!maps")
            {
                module = mapMod;
            }

            private enum MapSearchFilter
            { StartsWith, EndsWith, NoFilter, Contains };

            private bool MapNamePassesFilter(string MapName, MapSearchFilter FilterType, string Filter)
            {
                switch (FilterType)
                {
                    case MapSearchFilter.StartsWith:
                        return MapName.StartsWith(Filter);
                        break;

                    case MapSearchFilter.EndsWith:
                        return MapName.EndsWith(Filter);
                        break;

                    case MapSearchFilter.Contains:
                        return MapName.Contains(Filter);
                        break;

                    case MapSearchFilter.NoFilter:
                        return true;
                        break;
                }
                return true; //Why did we arrive here
            }

            private Tuple<string, string> GetMapsWithFilter(string Filter, MapSearchFilter FilterType, bool OnlyReturnUploadedMaps)
            {
                IReadOnlyList<Map> maps = module.mapList.GetAllMaps();

                if (maps.Count == 0)
                {
                    return new Tuple<string, string>("The map list is empty", " ");
                }
                else
                {
                    string chatResponse = "";
                    string pmResponse = "";

                    int MapsAddedToResponse = 0;
                    int MapsInResponseLimit = module.MaxMapNumber;

                    //Build Chat Response
                    for (int i = 0; i < maps.Count; i++)
                    {
                        if (OnlyReturnUploadedMaps && (module.CheckIfMapIsUploaded(maps[i].Filename) == false))
                        {
                            // do nothing
                        }
                        else if (MapNamePassesFilter(maps[i].Filename, FilterType, Filter))
                        {
                            if (MapsAddedToResponse < MapsInResponseLimit)
                            {
                                if (MapsAddedToResponse > 0)
                                {
                                    chatResponse += " , ";
                                }

                                chatResponse += maps[i].Filename;
                                MapsAddedToResponse++;
                            }
                            int Nextnum = i + 1;
                            string mapLine = string.Format(Nextnum + ") {0} // {1} // {2} ({3})", maps[i].Filename, maps[i].DownloadURL, maps[i].SubmitterName, maps[i].Submitter.ToString());

                            if (!string.IsNullOrEmpty(maps[i].Notes))
                                mapLine += "\nNotes: " + maps[i].Notes;

                            if (i < maps.Count - 1)
                                mapLine += "\n";

                            pmResponse += mapLine;
                        }
                    }

                    if (maps.Count > MapsAddedToResponse)
                    {
                        chatResponse += string.Format(" (and {0} more at: http://vbot.site )", maps.Count - MapsAddedToResponse);
                    }
                    else
                    {
                        chatResponse += " at: http://vbot.site";
                    }

                    if (MapsAddedToResponse == 0)
                    {
                        return new Tuple<string, string>("There were no maps found with those search terms!", "");
                    }

                    return new Tuple<string, string>(chatResponse, pmResponse);
                }
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                MapSearchFilter Filter = MapSearchFilter.NoFilter;
                string MapFilter = "";
                //If the param actually has a mapname, and isn't empty, execute the filtering system
                bool ValidData = (string.IsNullOrWhiteSpace(param) != true && param.StartsWith("!maps", StringComparison.OrdinalIgnoreCase) != true);
                bool OnlyWantUploaded = Msg.ReceivedMessage.StartsWith("!mapsuploaded", StringComparison.OrdinalIgnoreCase);

                if (ValidData)
                {
                    char asterisk = '*';
                    bool StartsWithAsterisk = param.StartsWith(asterisk.ToString());
                    bool EndsWithAsterisk = param.EndsWith(asterisk.ToString());

                    param.TrimStart(asterisk);
                    param.TrimEnd(asterisk);

                    if (StartsWithAsterisk)
                    {
                        Filter = MapSearchFilter.EndsWith;
                        param = param.Substring(1, param.Length - 1);
                    }
                    if (EndsWithAsterisk)
                    {
                        param = param.Substring(0, param.Length - 1);
                        Filter = MapSearchFilter.StartsWith;
                    }

                    if (StartsWithAsterisk == EndsWithAsterisk) //Either starts AND ends with asterisks, OR No Asterisks
                    {
                        Filter = MapSearchFilter.Contains;
                    }

                    MapFilter = param;
                }

                Tuple<string, string> Responses = GetMapsWithFilter(MapFilter, Filter, OnlyWantUploaded);

                userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = Responses.Item2 });

                if (DateTime.Now < LastExecuted.AddMinutes(1))
                {
                    userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = Responses.Item1 });

                    return null;
                }
                else
                {
                    LastExecuted = DateTime.Now;
                    return Responses.Item1;
                }
            }
        }

        private class Reposition : MapCommand
        {
            public Reposition(ModuleHandler bot, MapModule mapMod) : base(bot, "!reposition", mapMod, "!reposition <new position> <filename>")
            {
            }

            public override string runcommand(MessageEventArgs Msg, string param)
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
                        MapModule.mapList.RemoveMap(editedMap, "Map Repositioned");
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
            public Update(ModuleHandler bot, MapModule mapMod) : base(bot, "!update", mapMod, "!update <Current Filename> <New filename> <New Url> <new notes>")
            {
            }

            public override string runcommand(MessageEventArgs msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);

                if (parameters.Length > 1)
                {
                    Map NewMapdata = ParseStringToMap(parameters[1]);
                    if (userhandler.admincheck(msg.Sender))
                    {
                        msg.Sender.Rank = ChatroomEntity.AdminStatus.True;
                    }

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
            public Delete(ModuleHandler bot, MapModule mapMod) : base(bot, "!delete", mapMod, "!delete <filename> OR !delete <position> ")
            {
            }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);
                int MapPositionInList = 0;
                Map deletedMap = new Map();

                if (int.TryParse(parameters[0], out MapPositionInList))
                {
                    if ((MapPositionInList > MapModule.mapList.GetSize()) | (MapPositionInList <= 0))
                    {
                        return "That index does not exist! Please use a valid number when deleting maps";
                    }
                    else
                    {
                        MapPositionInList--;
                        deletedMap = MapModule.mapList.GetMap(MapPositionInList);
                    }
                }
                else
                {
                    deletedMap = MapModule.mapList.GetMapByFilename(parameters[0]);
                }

                if (deletedMap == null)
                {
                    return string.Format("Map '{0}' was not found.", parameters[0]);
                }
                else
                {
                    if ((deletedMap.IsOwner(Msg.Sender.identifier)) || (userhandler.admincheck(Msg.Sender)))
                    {
                        string Reason = "Deleted by " + Msg.Sender.DisplayName + " (" + Msg.Sender.identifier + "). ";
                        string ExplicitReason = param.Substring(parameters[0].Length, param.Length - parameters[0].Length);

                        if (!string.IsNullOrWhiteSpace(ExplicitReason))
                        {
                            Reason += "Reason given: " + ExplicitReason;
                        }
                        else
                        {
                            Reason += "No reason given";
                        }

                        userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = new User(deletedMap.Submitter, null), ReplyMessage = string.Format("Your map {0} has been deleted from the map list. {1}", deletedMap.Filename, Reason) });

                        MapModule.mapList.RemoveMap(deletedMap, Reason);
                        MapModule.savePersistentData();
                        return string.Format("Map '{0}' DELETED. Sending: {1}", deletedMap.Filename, Reason);
                    }
                    else
                    {
                        return string.Format("You do not have permission to edit map '{0}'.", deletedMap.Filename);
                    }
                }
            }
        }

        private class GetOwner : BaseCommand
        {
            private MapModule module;

            public GetOwner(ModuleHandler bot, MapModule mapMod) : base(bot, "!GetOwner")
            {
                module = mapMod;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length > 0)
                {
                    Map GetMap = module.mapList.GetMapByFilename(parameters[0]);

                    if (GetMap == null)
                    {
                        return string.Format("Map '{0}' was not found.", parameters[0]);
                    }
                    else
                    {
                        return string.Format("Map owner is: {0} Extra info: {1} | {2} | Owner: {3} ", GetMap.Submitter, GetMap.Submitter.ToString(), GetMap.SubmitterName, GetMap.IsOwner(Msg.Sender.identifier));
                    }
                }
                return "Invalid parameters for !GetOwner. Syntax: !GetOwner <filename>";
            }
        }

        private class Wipe : MapCommand
        {
            private MapModule module;

            public Wipe(ModuleHandler bot, MapModule mapMod) : base(bot, "!wipe", mapMod, "!wipe <reason>")
            {
                module = mapMod;
            }

            public override string runcommand(MessageEventArgs Msg, string param)
            {
                if (!string.IsNullOrEmpty(param))
                {
                    module.ClearMapListWithMessage(param);
                    module.savePersistentData();
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
                Msg.Destination = new User(mapList.GetMap(0).Submitter, null);
                userhandler.SendPrivateMessageProcessEvent(Msg);
                mapList.RemoveMap(0);
            };
            savePersistentData();
        }
    }
}