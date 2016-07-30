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

        public ServerModule(VBot bot, Dictionary<string, object> config) : base(bot, config)
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
                commands.Add(new Status(bot, server));

            commands.Add(new Active(bot, this));

            serverUpdate = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(SyncServerInfo));
        }

        public class ServerInfo : EventArgs
        {
            public IPEndPoint serverIP;
            public string tag;
            public int playerCount;
            public int capacity;
            public string currentMap;

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
            foreach (ServerInfo server in serverList)
            {
                ServerInfo serverstate = ServerQuery(server);

                if (serverstate != null)
                {
                    bool mapUpdated = serverstate.currentMap != server.currentMap && server.currentMap != string.Empty;
                    server.update(serverstate);

                    if (mapUpdated)
                        chatIsNotified = false;
                    
                    if (!chatIsNotified && server.playerCount > 3)
                    {
                        /* //TODO Re-Enabled
                        EventHandler handler = mapBeingTested; 

                        if (handler != null)
                        {
                            handler(this, server);
                        }
                        */
                        userhandler.steamConnectionHandler.SteamFriends.SendChatRoomMessage(userhandler.GroupChatSID, EChatEntryType.ChatMsg, server.ToString());
                        chatIsNotified = true;                        
                    }

                    if (ServerUpdated != null)
                        ServerUpdated(this, server);
                }
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
                client.Client.ReceiveTimeout = 3000;
                client.Client.SendTimeout = 3000;

                client.Connect(server.serverIP);

                var request = new List<byte>();
                request.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF, 0x54 });
                request.AddRange(Encoding.ASCII.GetBytes("Source Engine Query\0"));
                var requestArr = request.ToArray();
                client.Send(requestArr, requestArr.Length);
                var data = client.Receive(ref localEndpoint).Skip(6).ToArray();

                updatedServer = new ServerInfo(server.serverIP, server.tag);
                //byte[] data = client.EndReceive(Response, ref localEndpoint).Skip(6).ToArray();
                string[] serverinfos = Encoding.ASCII.GetString(data).Split(new char[] { '\0' }, 5);
                // getting and sanitizing map name
                updatedServer.currentMap = serverinfos[1].Split('.')[0].Replace("workshop/", "");
                // getting playerount
                updatedServer.playerCount = (int)Encoding.ASCII.GetBytes(serverinfos[4]).Skip(2).ToArray()[0];
                // getting server capacity
                updatedServer.capacity = (int)Encoding.ASCII.GetBytes(serverinfos[4]).Skip(2).ToArray()[1];
            }

            return updatedServer;
        }


        // Special status commands

        private sealed class Status : BaseCommand
        {
            // Automaticaly generated status command for each server under the config
            protected ServerInfo server;

            public Status(VBot bot, ServerInfo server) : base(bot, "!" + server.tag + "server")
            {
                this.server = server;
            }
            protected override string exec(SteamID sender, string param)
            {
                ServerInfo status = ServerQuery(server);
                if (status != null)
                {
                    server.update(status);
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
            protected override string exec(SteamID sender, string param)
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
