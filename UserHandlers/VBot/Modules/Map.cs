using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;

using SteamKit2;
using Newtonsoft.Json;

namespace SteamBotLite
{
    class MapModule : BaseModule
    {
       // public List<Map> mapList = new List<Map>();  //OLD MAP SYSTEM
        public ObservableCollection<Map> mapList = new ObservableCollection<Map>();

        int MaxMapNumber = 10; 

        public MapModule(VBot bot, Dictionary<string, object> config) : base(bot, config)
        {
            loadPersistentData();
            
            commands.Add(new Add(bot, this));
            commands.Add(new Maps(bot, this));
            commands.Add(new Update(bot, this));
            commands.Add(new Delete(bot, this));
            adminCommands.Add(new Wipe(bot, this));
        }

        public class Map
        {
            public uint Submitter { get; set; }
            public string Filename { get; set; }
            public string DownloadURL { get; set; }
            public string Notes { get; set; }
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
                mapList = JsonConvert.DeserializeObject<ObservableCollection<Map>>(System.IO.File.ReadAllText(this.GetType().Name + ".json"));
                Console.WriteLine("Loaded Map List");
            }
            catch
            {
                Console.WriteLine("Error Loading Map List");
            }
        }

        public void HandleEvent(object sender, ServerModule.ServerInfo args)
        {
            var map = mapList.FirstOrDefault(x => x.Filename == args.currentMap);
            if (map != null)
            {
                mapList.Remove(map);
                Console.WriteLine("Map {0} is being tested on the {1} server and has been removed.", map.Filename, args.tag);
            }
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

        // The commands

        private class Add : MapCommand
        {
            public Add(VBot bot, MapModule mapModule) : base(bot, "!add", mapModule) { }
            protected override string exec(SteamID sender, string param)
            {
                string[] parameters = param.Split(' ');
                Map map = new Map();
                if (parameters.Length > 0)
                {
                    map.Submitter = sender.AccountID;
                    map.Filename = parameters[0];
                    map.Notes = "No Notes";
                    if (parameters.Length > 1 /* && !uploaded*/)
                    {
                        if (parameters.Length > 2)
                        {
                            map.Notes = param.Substring(parameters[0].Length + parameters[1].Length);
                        }
                        map.DownloadURL = parameters[1];
                        MapModule.mapList.Add(map);
                        MapModule.savePersistentData();

                        return string.Format("Map '{0}' added.", map.Filename);
                    }
                }
                return "Invalid parameters for !add. Syntax: !add <mapname> <url> (notes)";
            }
        }

        private class Maps : MapCommand
        {
            public Maps(VBot bot, MapModule mapMod) : base(bot, "!maps", mapMod) { }
            protected override string exec(SteamID sender, string param)
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
                        chatResponse += string.Format(" (and {0} more...)", maps.Count - maxMaps);

                    // Build the private response.
                    pmResponse = "";
                    for (int i = 0; i < maps.Count; i++)
                    {
                        string mapLine = string.Format("{0} // {1}", maps[i].Filename, maps[i].DownloadURL);

                        if (!string.IsNullOrEmpty(maps[i].Notes))
                            mapLine += "\nNotes: " + maps[i].Notes;

                        if (i < maps.Count - 1)
                            mapLine += "\n";

                        pmResponse += mapLine;
                    }
                }

                // PM map list to the caller.
                if (maps.Count != 0)
                    userhandler.steamConnectionHandler.SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg, pmResponse);

                return chatResponse;
            }
        }

        private class Update : MapCommand
        {
            public Update(VBot bot, MapModule mapMod) : base(bot, "!update", mapMod) { }
            protected override string exec(SteamID sender, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length < 1)
                {
                    return string.Format("Invalid parameters for !update. Syntax: !update <mapname> (url)");
                }
                else
                {
                    Map editedMap = MapModule.mapList.Where(x => x.Filename.Equals(parameters[0])).FirstOrDefault(); //Needs to be tested
                    // Map editedMap = MapModule.mapList.Find(map => map.filename.Equals(parameters[0])); //OLD Map CODE
                    if (editedMap.Submitter == sender.AccountID)
                    {
                        MapModule.mapList.Remove(editedMap);

                        editedMap.Filename = parameters[1];
                        if (parameters.Length > 2)
                            editedMap.DownloadURL = parameters[2];
                        MapModule.mapList.Add(editedMap);
                        MapModule.savePersistentData();
                        return string.Format("Map '{0}' has been edited.", editedMap.Filename);
                    }
                    else
                    {
                        return string.Format("You cannot edit map '{0}' as you did not submit it.", editedMap.Filename);
                    }
                }
            }
        }

        private class Delete : MapCommand
        {
            public Delete(VBot bot, MapModule mapMod) : base(bot, "!delete", mapMod) { }
            protected override string exec(SteamID sender, string param)
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
                        if ((deletedMap.Submitter == sender.AccountID) || (userhandler.usersModule.admincheck(sender.AccountID)))
                        {
                            MapModule.mapList.Remove(deletedMap);
                            MapModule.savePersistentData();
                            return string.Format("Map '{0}' DELETED.", deletedMap.Filename);
                        }
                        else
                        {
                            return string.Format("You do not have permission to edit map '{0}'.", deletedMap.DownloadURL);
                        }
                    }
                }
                return "Invalid parameters for !delete. Syntax: !delete <mapname>";
            }
            
        }

        private class Wipe : MapCommand
        {
            public Wipe(VBot bot, MapModule mapMod) : base(bot, "!wipe", mapMod) { }
            protected override string exec(SteamID sender, string param)
            {
                MapModule.mapList.Clear(); 
                //MapModule.mapList = new List<Map>(); //OLd Maplist code
                MapModule.savePersistentData();
                return "The map list has been DELETED.";
            }
        }
    }
}
