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
    class ServerModule : BaseModule
    {
        public event EventHandler<ServerInfo> ServerUpdated;


        private BaseTask serverUpdate;

        public ServerList serverList;

        public VBot Bot;

        public class ServerList
        {
            ServerModule Servermodule;

            public ServerList(ServerModule module , List<ServerInfo> serverlist)
            {
                Servermodule = module;
                ServerListObject = serverlist;
            }

            List<ServerInfo> ServerListObject { get; }

            public IReadOnlyList<ServerInfo> Servers
            {
                get
                {
                    return ServerListObject.AsReadOnly();
                }
            }

            public void Add (ServerInfo server)
            {
                ServerListObject.Add(server);
                Servermodule.commands.Add((new Status(Servermodule.Bot, server, Servermodule)));
                Servermodule.savePersistentData();
            }
            public void Remove (ServerInfo server)
            {
                
                foreach (BaseCommand command in Servermodule.commands)
                {
                    if(command.command.Equals("!" + server.tag + "server"))
                    {
                        Servermodule.commands.Remove(command);
                    }
                }
                ServerListObject.Remove(server);

                Servermodule.savePersistentData();
            }
        }
       
        public ServerModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            
            Bot = bot;
            List<ServerInfo> ServerList = new List<ServerInfo>();

            // loading config

            int updateInterval = int.Parse(config["updateInterval"].ToString());
            Tuple<string, string, int>[] servers;

            if (File.Exists(ModuleSavedDataFilePath()))
            {
                ServerList = JsonConvert.DeserializeObject<List<ServerInfo>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                if (ServerList.Count > 0)
                {
                    foreach (ServerInfo server in ServerList)
                    {
                        commands.Add(new Status(bot, server, this));
                    }
                }
            }
            else
            {
                Console.WriteLine("No servers found in config file");
                servers = null;
            }

            serverList = new ServerList(this, ServerList);
            commands.Add(new Active(bot, this));
            adminCommands.Add(new ServerAdd(bot, this));
            adminCommands.Add(new ServerRemove(bot, this));
            adminCommands.Add(new FullServerQuery(bot, this));
            

            serverUpdate = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(SyncServerInfo));
            ServerUpdated += bot.ServerUpdated;
        }

        public class ServerInfo : EventArgs
        {
            public string serverIP; 
            public int port;

            public string tag;
            public int playerCount;
            public int capacity;
            public string currentMap = "";

            public ServerInfo(string serverIP, int port, string tag)
            {
                this.serverIP = serverIP;
                this.port = port;
                this.tag = tag;
            }

            public void update(ServerInfo updated)
            {
                this.playerCount = updated.playerCount;
                this.capacity = updated.capacity;
                this.currentMap = updated.currentMap;
            }

            public override string ToString()
            {
                return this.tag + " server is now on " + this.currentMap +
                    " - " + this.playerCount + "/" + this.capacity +
                    " - join: steam://connect/" + this.serverIP + ":" + this.port;
            }
        }

        public void SyncServerInfo(object sender, EventArgs e)
        {
            int x = 0;
            foreach (ServerInfo server in serverList.Servers)
            {
                ServerInfo serverstate = ServerQuery(server);

                if (serverstate != null)
                {
                   // Console.WriteLine(string.Format("New Map is {0} Oldy one is {1} and the player count is {2} went from {3}", serverstate.currentMap, serverList[x].currentMap, serverstate.playerCount, serverList[x].playerCount));
                    
                    if (!(serverList.Servers[x].currentMap.Equals(serverstate.currentMap)) && (serverstate.playerCount > 3))
                    {
                        serverList.Servers[x].update(serverstate);
                        ServerUpdated(this, serverstate);
                        userhandler.BroadcastMessageProcessEvent(serverstate.ToString());
                    }
                }
                x++;
            }
        }

        public override string getPersistentData()
        {
            return JsonConvert.SerializeObject(serverList.Servers);
        }

        public override void loadPersistentData()
        {
            try
            {
                Tuple<string, string, int>[] servers = JsonConvert.DeserializeObject<Tuple<string, string, int>[]>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                // parsing ServerInfos
                foreach (Tuple<string, string, int> servconf in servers)
                {
                    IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse(servconf.Item2), servconf.Item3);
                    ServerInfo serverInfo = new ServerInfo(servconf.Item2,servconf.Item3 , servconf.Item1);
                    serverList.Add(serverInfo);
                }
            }
            catch
            {
                Console.WriteLine("There was an error loading the serverlist");
            }
        }

        // queries a server and returns a <string, int> Tuple (filename, playercount)
        public static ServerInfo ServerQuery(ServerInfo server)
        {
            ServerInfo updatedServer = null;
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

                    updatedServer = new ServerInfo(server.serverIP, server.port, server.tag);
                    string[] serverinfos = Encoding.ASCII.GetString(data).Split(new char[] { '\0' }, 5);
                    // getting and sanitizing filename
                    updatedServer.currentMap = serverinfos[1].Split('.')[0].Replace("workshop/", "");
                    // getting playerount
                    updatedServer.playerCount = (int)Encoding.ASCII.GetBytes(serverinfos[4]).Skip(2).ToArray()[0];
                    // getting server capacity
                    updatedServer.capacity = (int)Encoding.ASCII.GetBytes(serverinfos[4]).Skip(2).ToArray()[1];

                    //Console.WriteLine(string.Format("{0} Responded with {1} and {2}",updatedServer.tag, updatedServer.currentMap,updatedServer.playerCount));

                    //Console.WriteLine(string.Format("{0} Responded with: {1} and: {2}", updatedServer.tag, updatedServer.currentMap, updatedServer.playerCount));

                   // Console.WriteLine(string.Format("{0} Responded with {1}",server.tag, updatedServer.currentMap));

                    client.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(string.Format("Error from: {0}: {1}",server.tag, ex.Message));  
                    client.Close();                  
                }
            }

            return updatedServer;
        }


        // Special status commands

        private sealed class Status : BaseCommand
        {
            // Automaticaly generated status command for each server under the config
            ServerInfo server;
            ServerModule servermodule;

            public Status(VBot bot, ServerInfo server, ServerModule module) : base(bot, "!" + server.tag.ToLower() + "server")
            {
                this.server = server;
                servermodule = module;

            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                ServerInfo status = ServerQuery(server);
                if (status != null)
                {
                    server.update(status);
                  //  servermodule.ServerUpdated(this, server);
                    return server.ToString();
                }
                else
                    return server.tag + " server did not respond";
            }
        }

        private class ServerAdd : BaseCommand
        {
            ServerModule module;
            public ServerAdd(VBot bot, ServerModule module) : base(bot, "!serveradd")
            {
                this.module = module;

            }

            protected override string exec(MessageProcessEventData Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' });
                if (parameters.Length > 2)
                {
                    try
                    {
                        IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse(parameters[1]), int.Parse(parameters[2]));
                        ServerInfo Server = new ServerInfo(parameters[1], int.Parse(parameters[2]), parameters[0]);
                        module.serverList.Add(Server);

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
            ServerModule module;

            public ServerRemove(VBot bot, ServerModule module) : base(bot, "!serverremove")
            {
                this.module = module;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                foreach (ServerInfo server in module.serverList.Servers)
                    {
                        if (param.Equals(server.tag, StringComparison.OrdinalIgnoreCase))
                        {
                            module.serverList.Remove(server);
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
            ServerModule module;
            public Active(VBot bot, ServerModule module) : base(bot, "!Active")
            {
                this.module = module;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                string activeServers = "";
                foreach (ServerInfo server in module.serverList.Servers)
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
            ServerModule module;
            public FullServerQuery(VBot bot, ServerModule module) : base(bot, "!Serverquery")
            {
                this.module = module;
            }
            protected override string exec(MessageProcessEventData Msg, string param)
            {
                string activeServers = "";
                foreach (ServerInfo server in module.serverList.Servers)
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
