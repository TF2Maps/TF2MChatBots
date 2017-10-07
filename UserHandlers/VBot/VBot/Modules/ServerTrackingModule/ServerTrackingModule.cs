using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;

namespace SteamBotLite
{
    public class ServerTrackingModule : BaseModule
    {
        public event EventHandler<TrackingServerInfo> ServerMapChanged;


        private BaseTask serverUpdate;

        public TrackingServerList TrackedServers;

        public ModuleHandler Bot;

        HTMLFileFromArrayListiners WebServer;

        public override void OnAllModulesLoaded() { }


        public ServerTrackingModule(ModuleHandler bot, HTMLFileFromArrayListiners WebServer, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            this.WebServer = WebServer;
            Bot = bot;
            List<TrackingServerInfo> ServersBeingTracked = new List<TrackingServerInfo>();

            int updateInterval = int.Parse(config["updateInterval"].ToString());
            Tuple<string, string, int>[] servers;

            if (File.Exists(ModuleSavedDataFilePath()))
            {
                ServersBeingTracked = JsonConvert.DeserializeObject<List<TrackingServerInfo>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                for (int i = 0; i < ServersBeingTracked.Count; i++)
                {
                    commands.Add(new Status(bot, ServersBeingTracked[i], this));
                }
            }
            else
            {
                Console.WriteLine("No servers found in config file");
                servers = null;
            }

            TrackedServers = new TrackingServerList(this, ServersBeingTracked);

            commands.Add(new Active(bot, this));
            commands.Add(new SpecificServerStatus(bot, this));
            adminCommands.Add(new ServerAdd(bot, this));
            adminCommands.Add(new ServerRemove(bot, this));
            adminCommands.Add(new FullServerQuery(bot, this));

            serverUpdate = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(SyncTrackingServerInfo));

            ServerMapChanged += bot.ServerUpdated;
            ServerMapChanged += ServerTrackingModule_ServerMapChanged;
        }

        public string NameToserverCommand (string servername)
        {
            return "!" + servername.ToLower() + "server";
        }

        private void ServerTrackingModule_ServerMapChanged(object sender, TrackingServerInfo e)
        {

            string TableLabel = e.tag + " History";
            

            TableDataValue HeaderName = new TableDataValue();
            HeaderName.VisibleValue = "Map Name";

            TableDataValue HeaderNamePlayerCount = new TableDataValue();
            HeaderNamePlayerCount.VisibleValue = "PlayerCount";

            TableDataValue HeaderTime = new TableDataValue();
            HeaderTime.VisibleValue = "Time (UTC)";

            WebServer.SetTableHeader(TableLabel, new TableDataValue[] { HeaderName, HeaderNamePlayerCount, HeaderTime });

            //Add Entry

            TableDataValue MapName = new TableDataValue();
            MapName.VisibleValue = e.currentMap;

            TableDataValue PlayerCount = new TableDataValue();
            PlayerCount.VisibleValue = e.playerCount.ToString();

            TableDataValue Time = new TableDataValue();
            Time.VisibleValue = DateTime.UtcNow.ToLongDateString() + " " + DateTime.UtcNow.ToLongTimeString();

            WebServer.AddEntryWithLimit(TableLabel, new TableDataValue[] { MapName, PlayerCount, Time }, 10);

            if (e.playerCount > 8) {
                userhandler.BroadcastMessageProcessEvent(e.ToString());

                TableDataValue HeaderServer = new TableDataValue();
                HeaderServer.VisibleValue = "Server";

                TableDataValue ServerLabel = new TableDataValue();
                ServerLabel.VisibleValue = e.tag;
                ServerLabel.Link = "steam://connect/" + e.serverIP + ":" + e.port;

                string RecentlyTestedTableLabel = "Recently Tested";
                WebServer.SetTableHeader(RecentlyTestedTableLabel, new TableDataValue[] { HeaderName, HeaderNamePlayerCount, HeaderTime,  HeaderServer });
                WebServer.AddEntryWithLimit(RecentlyTestedTableLabel, new TableDataValue[] { MapName, PlayerCount, Time, ServerLabel}, 10);
            }

        }

        

        public void SyncTrackingServerInfo(object sender, EventArgs e)
        {
            for (int x = 0; x < TrackedServers.Servers.Count; x++)
            {
                TrackingServerInfo currentserverstate = ServerQuery(TrackedServers.Servers[x]);

                if (currentserverstate != null)
                {
                    if (!(TrackedServers.Servers[x].currentMap.Equals(currentserverstate.currentMap)))
                    {
                        ServerMapChanged(this, currentserverstate);
                    }
                    TrackedServers.Servers[x].update(currentserverstate);
                }
            }
        }

        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(TrackedServers.Servers);
        }

        public override void loadPersistentData()
        {
            try
            {
                Tuple<string, string, int>[] servers = JsonConvert.DeserializeObject<Tuple<string, string, int>[]>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));

                foreach (Tuple<string, string, int> servconf in servers)
                {
                    IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse(servconf.Item2), servconf.Item3);
                    TrackingServerInfo TrackingServerInfo = new TrackingServerInfo(servconf.Item2, servconf.Item3, servconf.Item1);
                    TrackedServers.Add(TrackingServerInfo);
                }
            }
            catch
            {
                Console.WriteLine("There was an error loading the TrackingServerList");
            }
        }

        
        // queries a server and returns a <string, int> Tuple (filename, playercount)
        public  TrackingServerInfo ServerQuery(TrackingServerInfo server)
        {
            if (IsEmulating) //For testing purposes, we can assign and set an emulated TrackingServerInfo response
            {
                return EmulatedTrackingServerInfo;
            }
            else
            {
                TrackingServerInfo updatedServer = null;
                // request server infos
                IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 0);

                using (var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
                {
                    client.Client.ReceiveTimeout = 5000;
                    client.Client.SendTimeout = 5000;

                    client.Connect(new IPEndPoint(System.Net.IPAddress.Parse(server.serverIP), server.port));

                    var request = new List<byte>();
                    request.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x54 });
                    request.AddRange(Encoding.ASCII.GetBytes("Source Engine Query\0"));
                    var requestArr = request.ToArray();
                    client.Send(requestArr, requestArr.Length);

                    try
                    {
                        var data = client.Receive(ref localEndpoint).Skip(6).ToArray();

                        updatedServer = new TrackingServerInfo(server.serverIP, server.port, server.tag);
                        string[] TrackingServerInfos = Encoding.ASCII.GetString(data).Split(new char[] { '\0' }, 5);
                        // getting and sanitizing filename
                        updatedServer.currentMap = TrackingServerInfos[1].Split('.')[0].Replace("workshop/", "");
                        // getting playerount
                        updatedServer.playerCount = (int)Encoding.ASCII.GetBytes(TrackingServerInfos[4]).Skip(2).ToArray()[0];
                        // getting server capacity
                        updatedServer.capacity = (int)Encoding.ASCII.GetBytes(TrackingServerInfos[4]).Skip(2).ToArray()[1];

                        client.Close();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(string.Format("Error from: {0}: {1}", server.tag, ex.Message));
                        client.Close();
                    }
                }

                return updatedServer;
            }
        }

        //For Testing Purposes
        private TrackingServerInfo EmulatedTrackingServerInfo;
        private bool IsEmulating = false;

        public void EmulateServerQuery(TrackingServerInfo Response)
        {
            EmulatedTrackingServerInfo = Response;
            IsEmulating = true;
        }

        // Special status commands

        private sealed class Status : BaseCommand
        {
            // Automaticaly generated status command for each server under the config
            TrackingServerInfo server;
            ServerTrackingModule ServerTrackingModule;

            public Status(ModuleHandler bot, TrackingServerInfo server, ServerTrackingModule module) : base(bot, module.NameToserverCommand(server.tag))
            {
                this.server = server;
                ServerTrackingModule = module;

            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                TrackingServerInfo status = ServerTrackingModule.ServerQuery(server);
                if (status != null)
                {
                    server.update(status);
                    return server.ToString();
                }
                else
                    return server.tag + " server did not respond";
            }
        }

        private sealed class SpecificServerStatus : BaseCommand
        {
            // Automaticaly generated status command for each server under the config
            ServerTrackingModule ServerTrackingModule;

            public SpecificServerStatus(ModuleHandler bot, ServerTrackingModule module) : base(bot, "")
            {
                ServerTrackingModule = module;

            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                foreach (TrackingServerInfo server in ServerTrackingModule.TrackedServers)
                {
                    if (server.tag.Equals(param))
                    {
                        return ServerTrackingModule.ServerQuery(server).ToString();
                    }
                }
                return "An error occured when querying the server!";
            }

            public override bool CheckCommandExists(MessageEventArgs Msg, string Message)
            {
                foreach (TrackingServerInfo server in ServerTrackingModule.TrackedServers)
                {
                    if (Message.StartsWith(ServerTrackingModule.NameToserverCommand(server.tag), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                }
                return false;
            }
            
            public override string[] GetCommmand()
            {
                int AmountOfServers = ServerTrackingModule.TrackedServers.Count();
                string[] TrackingServerList = new string[AmountOfServers];

                for (int x = 0; x < AmountOfServers; x++ )
                {
                    TrackingServerList[x] = ServerTrackingModule.NameToserverCommand(ServerTrackingModule.TrackedServers.Servers[x].tag);
                }

                return TrackingServerList;
            }
        }

        private class ServerAdd : BaseCommand
        {
            ServerTrackingModule module;
            public ServerAdd(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!serveradd")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' });

                if (parameters.Length > 2)
                {
                    try
                    {
                        IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse(parameters[1]), int.Parse(parameters[2]));
                        TrackingServerInfo Server = new TrackingServerInfo(parameters[1], int.Parse(parameters[2]), parameters[0]);
                        module.TrackedServers.Add(Server);

                        return string.Format("Server {0} has been successfully added at: {1}", Server.tag, Server.serverIP);
                    }
                    catch
                    {
                        return "Your data types were invalid!";
                    }
                }
                else
                {
                    return "Your Server was not added, remember the command is: !serveradd servername serverIP serverPort";
                }

            }
        }
        private class ServerRemove : BaseCommand
        {
            ServerTrackingModule module;

            public ServerRemove(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!serverremove")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                foreach (TrackingServerInfo server in module.TrackedServers.Servers)
                {
                    if (param.Equals(server.tag, StringComparison.OrdinalIgnoreCase))
                    {
                        module.TrackedServers.Remove(server);
                        return "The server has been removed from the list";
                    }
                }
                return "Server was not found, remember the servername does not include ! preceeding or 'server' afterwards (EUserver would be EU)";
            }
        }

        // Other commands

        private class Active : BaseCommand
        {
            // Command to query if a server is active
            ServerTrackingModule module;
            public Active(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!Active")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string activeServers = "";
                foreach (TrackingServerInfo server in module.TrackedServers.Servers)
                    if (server.playerCount > 1)
                    {
                        if (!activeServers.Equals(string.Empty))
                            activeServers += "\n";

                        activeServers += server.ToString();
                    }
                return activeServers.Equals(string.Empty) ? "No server is currently active" : activeServers;
            }
        }

        private class FullServerQuery : BaseCommand
        {
            // Command to query if a server is active
            ServerTrackingModule module;
            public FullServerQuery(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!Serverquery")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string activeServers = "";
                foreach (TrackingServerInfo server in module.TrackedServers.Servers)
                {
                    if (!activeServers.Equals(string.Empty))
                    {
                        activeServers += "\n";
                    }
                    activeServers += server.ToString();
                }
                return activeServers.Equals(string.Empty) ? "No server is currently active" : activeServers;
            }
        }



    }
}
