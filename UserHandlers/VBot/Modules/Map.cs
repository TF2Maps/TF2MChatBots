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
        

        public MapModule(VBot bot, Dictionary<string, object> config) : base(bot, config)
        {
            loadPersistentData();
            
            commands.Add(new Add(bot, this));
            commands.Add(new Maps(bot, this));
            commands.Add(new Update(bot, this));
            commands.Add(new Delete(bot, this));
            adminCommands.Add(new Wipe(bot, this));
        }

        public struct Map
        {
            public uint submitter;
            public string filename;
            public string downloadURL;
            public string notes;
        }

        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(mapList);
        }

        public override void loadPersistentData()
        {
            try
            {ObservableCollection<Map>
                mapList = JsonConvert.DeserializeObject<ObservableCollection<Map>>(System.IO.File.ReadAllText(this.GetType().Name + ".json"));
            }
            catch { }
        }

        public void HandleEvent(object sender, EventArgs args)
        {
            ServerModule.ServerInfo imp = (ServerModule.ServerInfo) args;
          
            //mapList.RemoveAll(x => x.filename == imp.currentMap); //OLD MAP SYSTEM
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
                    map.submitter = sender.AccountID;
                    map.filename = parameters[0];
                    map.notes = "No Notes";
                    if (parameters.Length > 1 /* && !uploaded*/)
                    {
                        if (parameters.Length > 2)
                        {
                            map.notes = param.Substring(parameters[0].Length + parameters[1].Length);
                        }
                        map.downloadURL = parameters[1];
                        MapModule.mapList.Add(map);
                        MapModule.savePersistentData();
                        
                        return "map added";
                    }
                }
                return "map not added";
            }
        }

        private class Maps : MapCommand
        {
            public Maps(VBot bot, MapModule mapMod) : base(bot, "!maps", mapMod) { }
            protected override string exec(SteamID sender, string param)
            {
                string maplist = "";
                string extralist = "";
                foreach (Map m in MapModule.mapList)
                {
                    if (maplist != string.Empty)
                    {
                        maplist += " , ";
                        extralist += " \n ";
                    }
                    maplist += m.filename;
                    extralist += m.downloadURL + " Note: " + m.notes;
                }
                if (maplist.Equals(""))
                    return "The list is empty";
                userhandler.steamConnectionHandler.SteamFriends.SendChatMessage(sender, EChatEntryType.ChatMsg, extralist);
                return maplist;
            }
        }

        private class Update : MapCommand
        {
            public Update(VBot bot, MapModule mapMod) : base(bot, "!update", mapMod) { }
            protected override string exec(SteamID sender, string param)
            {
                string[] parameters = param.Split(' ');

                if (parameters.Length > 1)
                {

                    Map editedMap = MapModule.mapList.Where(x => x.filename.Equals(parameters[0])).FirstOrDefault(); //Needs to be tested 
                   // Map editedMap = MapModule.mapList.Find(map => map.filename.Equals(parameters[0])); //OLD Map CODE
                    if (editedMap.submitter == sender.AccountID)
                    {
                        MapModule.mapList.Remove(editedMap);

                        editedMap.filename = parameters[1];
                        if (parameters.Length > 2)
                            editedMap.downloadURL = parameters[2];
                        MapModule.mapList.Add(editedMap);
                        MapModule.savePersistentData();
                        return "map edited";
                    }
                }
                return "map not found or insufisant priviledge";
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
                    Map deletedMap = MapModule.mapList.Where(x => x.filename.Equals(parameters[0])).FirstOrDefault(); //Needs to be tested 
              //      Map deletedMap = MapModule.mapList.Find(map => map.filename.Equals(parameters[0]));
                    if ((deletedMap.submitter == sender.AccountID) || (userhandler.usersModule.admincheck(sender.AccountID)))
                    {
                        MapModule.mapList.Remove(deletedMap);
                        MapModule.savePersistentData();
                        return "Map DELETED";
                    }
                }
                return "map not found or insufisant priviledge";
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
                return "The map list, has been DELETED";
            }
        }
    }
}
