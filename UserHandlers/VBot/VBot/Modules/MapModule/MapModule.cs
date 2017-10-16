using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SteamBotLite
{
    public partial class MapModule : BaseModule, ServerMapChangeListiner
    {
        // public List<Map> mapList = new List<Map>();  //OLD MAP SYSTEM

        public MapCollection mapList;

        private IHTMLFileFromArrayListiners HTMLlistiner;
        private string MapListUploadCheck;
        private int MaxMapNumber = 10;
        private string ServerMapListUrl;

        private bool SubstitutingWebPage = false;

        private string TableName = "Current Maps";

        public MapModule(VBot bot, IHTMLFileFromArrayListiners HtmlListiner, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            this.HTMLlistiner = HtmlListiner;
            LoadModule(bot);
        }

        public MapModule(ModuleHandler bot, IHTMLFileFromArrayListiners HtmlListiner, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            this.HTMLlistiner = HtmlListiner;
            LoadModule(bot);
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

        public override void OnAllModulesLoaded()
        {
            userhandler.OnMaplistchange(mapList.GetAllMaps(), null, null);
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

        //This exists for testing purposes, Allowing us to emulate a webpage being returned
        public void SubstituteWebPageWithString(string data)
        {
            SubstitutingWebPage = true;
            MapListUploadCheck = data;
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

                HTMLlistiner.AddWebsiteEntry(TableName, Values, 0);
            }

            TableData data = new TableData();
            data.Header = GetMapListTableHeader();
            data.TableValues = Entries;

            HTMLlistiner.MakeTableFromEntry(TableName, data);
        }

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
            commands.Add(new GetMaps(bot, this));
            commands.Add(new UpdateMap(bot, this));
            commands.Add(new UpdateName(bot, this));
            commands.Add(new DeleteMaps(bot, this));
            commands.Add(new UploadCheck(bot, this));
            adminCommands.Add(new InsertMap(bot, this));
            adminCommands.Add(new RepositionMap(bot, this));
            adminCommands.Add(new WipeMaps(bot, this));
            adminCommands.Add(new AllowOnlyUploadedMapsSetter(bot, this));
            commands.Add(new GetOwner(bot, this));

            bot.AddMapChangeEventListiner(this);
        }

        private void MapChange(object sender, NotifyCollectionChangedEventArgs args)
        {
            userhandler.OnMaplistchange(mapList.GetAllMaps(), sender, args);
            ConvertMaplistToTable();
        }

        abstract public class MapCommand : BaseCommand
        {
            protected MapModule MapModule;
            private string command;
            private string syntax;

            public MapCommand(ModuleHandler bot, string command, MapModule mapMod, string Syntax) : base(bot, command)
            {
                this.MapModule = mapMod;
                this.syntax = Syntax;
                this.command = command;
            }

            public Map CleanupAndParseMsgToMap(MessageEventArgs Msg, string param)
            {
                string message = RemoveWhiteSpacesFromString(param);

                Map map = ParseStringToMap(message);
                map.Submitter = Msg.Sender.identifier;
                map.SubmitterName = Msg.Sender.DisplayName;
                map.SubmitterContact = Msg.Sender.UserURL;

                return map;
            }

            public bool MapIsUploadedToWebsite(string filename)
            {
                return MapModule.CheckIfMapIsUploaded(filename);
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

            public abstract string runcommand(MessageEventArgs Msg, string param);

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
        }

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
    }
}