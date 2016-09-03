using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;

namespace SteamBotLite
{
    class ServerModule : BaseModule
    {
        public event EventHandler<ServerInfo> ServerUpdated;

        public List<ServerInfo> serverList;
        private BaseTask serverUpdate;
        bool chatIsNotified = true;

        public ServerModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            serverList = new List<ServerInfo>();

            // loading config
            int updateInterval = int.Parse(config["updateInterval"].ToString());
            Tuple<string, string, int>[] servers = JsonConvert.DeserializeObject<Tuple<string, string, int>[]>(config["Servers"].ToString());

            // parsing ServerInfos
            foreach (Tuple<string, string, int> servconf in servers)
            {
                IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse(servconf.Item2), servconf.Item3);
                ServerInfo serverInfo = new ServerInfo(ep, servconf.Item1);
                serverList.Add(serverInfo);
            }

            // loading commands
            foreach (ServerInfo server in serverList)
                commands.Add(new Status(bot, server,this));

            commands.Add(new Active(bot, this));

            serverUpdate = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(SyncServerInfo));
            ServerUpdated += bot.ServerUpdated;
        }

        public class ServerInfo : EventArgs
        {
            public IPEndPoint serverIP;
            public string tag;
            public int playerCount;
            public int capacity;
            public string currentMap = "";

            public ServerInfo(IPEndPoint serverIP, string tag)
            {
                this.serverIP = serverIP;
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
                    " - join: steam://connect/" + this.serverIP;
            }
        }

        public void SyncServerInfo(object sender, EventArgs e)
        {
            int x = 0;
            foreach (ServerInfo server in serverList)
            {
                ServerInfo serverstate = ServerQuery(server);

                if (serverstate != null)
                {
                    Console.WriteLine(string.Format("New Map is {0} Oldy one is {1} and the player count is {2} went from {3}", serverstate.currentMap, serverList[x].currentMap, serverstate.playerCount, serverList[x].playerCount));
                    
                    if (!(serverList[x].currentMap.Equals(serverstate.currentMap)) && (serverstate.playerCount > 3))
                    {
                        serverList[x].update(serverstate);
                        ServerUpdated(this, serverstate);
                        userhandler.appinterface.SendChatRoomMessage(userhandler.GroupChatSID,serverstate.ToString());
                    }
                }
                x++;
            }
        }

        public override string getPersistentData()
        {
            return "";
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }

        // queries a server and returns a <string, int> Tuple (mapname, playercount)
        public static ServerInfo ServerQuery(ServerInfo server)
        {
            ServerInfo updatedServer = null;
            // request server infos
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 0);

            using (var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
            {
                client.Client.ReceiveTimeout = 5000;
                client.Client.SendTimeout = 5000;

                client.Connect(server.serverIP);

                var request = new List<byte>();
                request.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x54 });
                request.AddRange(Encoding.ASCII.GetBytes("Source Engine Query\0"));
                var requestArr = request.ToArray();
                client.Send(requestArr, requestArr.Length);

                try
                {
                    var data = client.Receive(ref localEndpoint).Skip(6).ToArray();

                    updatedServer = new ServerInfo(server.serverIP, server.tag);
                    string[] serverinfos = Encoding.ASCII.GetString(data).Split(new char[] { '\0' }, 5);
                    // getting and sanitizing map name
                    updatedServer.currentMap = serverinfos[1].Split('.')[0].Replace("workshop/", "");
                    // getting playerount
                    updatedServer.playerCount = (int)Encoding.ASCII.GetBytes(serverinfos[4]).Skip(2).ToArray()[0];
                    // getting server capacity
                    updatedServer.capacity = (int)Encoding.ASCII.GetBytes(serverinfos[4]).Skip(2).ToArray()[1];

                    Console.WriteLine(string.Format("{0} Responded with {1} and {2}",updatedServer.tag, updatedServer.currentMap,updatedServer.playerCount));

                    Console.WriteLine(string.Format("{0} Responded with: {1} and: {2}", updatedServer.tag, updatedServer.currentMap, updatedServer.playerCount));

                    Console.WriteLine(string.Format("{0} Responded with {1}",server.tag, updatedServer.currentMap));

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
            protected ServerInfo server;
            ServerModule servermodule;

            public Status(VBot bot, ServerInfo server, ServerModule module) : base(bot, "!" + server.tag + "server")
            {
                this.server = server;
                servermodule = module;

            }
            protected override string exec(UserIdentifier sender, string param)
            {
                ServerInfo status = ServerQuery(server);
                if (status != null)
                {
                    server.update(status);
                    servermodule.ServerUpdated(this, server);
                    return server.ToString();
                }
                else
                    return server.tag + " server did not respond";
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
            protected override string exec(UserIdentifier sender, string param)
            {
                string activeServers = "";
                foreach (ServerInfo server in module.serverList)
                    if (server.playerCount > 1)
                    {
                        if (!activeServers.Equals(string.Empty))    
                            activeServers += "\n";

                        activeServers += server.ToString();
                    }
                return activeServers.Equals(string.Empty) ? "no server is currently active" : activeServers;
            }
        }
    }
}
