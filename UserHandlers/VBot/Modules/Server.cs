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
        public List<ServerInfo> serverList;
        private BaseTask serverUpdate;

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

        public class ServerInfo
        {
            public IPEndPoint serverIP;
            public string tag;
            public int playerCount;
            public string currentMap;

            public ServerInfo(IPEndPoint serverIP, string tag)
            {
                this.serverIP = serverIP;
                this.tag = tag;
                currentMap = "";
                playerCount = 0;
            }
        }

        public void SyncServerInfo(object sender, EventArgs e)
        {
            Tuple<string, int> serverstate; // map, playercount
            foreach (ServerInfo server in serverList)
            {
                serverstate = ServerQuery(server.serverIP);
                if (serverstate != null)
                {
                    server.playerCount = serverstate.Item2;
                    if (!serverstate.Item1.Equals(server.currentMap) && !server.currentMap.Equals(string.Empty))
                    {
                        string message = server.tag + " server is now playing " + serverstate.Item1 + " with " + serverstate.Item2 + " players" ;
                        userhandler.steamConnectionHandler.SteamFriends.SendChatRoomMessage(userhandler.GroupChatSID, EChatEntryType.ChatMsg, message);
                    }
                    server.currentMap = serverstate.Item1;
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
        static public Tuple<string, int> ServerQuery(IPEndPoint serverIP)
        {
            
            // request server infos
            IPEndPoint localEndpoint = new IPEndPoint(IPAddress.Any, 0);
            UdpClient client = new UdpClient(localEndpoint);
            client.Connect(serverIP);
            
            byte[] header = new byte[] {0xFF, 0xFF, 0xFF, 0xFF, 0x54};
            byte[] request = header.Concat(Encoding.ASCII.GetBytes("Source Engine Query\0")).ToArray();
            client.Send(request, request.Length);
            
            // get response (timeout after 3 seconds)and skip header
            var Response = client.BeginReceive(null, null);
            Response.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));
            // Response.AsyncWaitHandle.Close();
            
            
            if (Response.IsCompleted)
            {
                byte[] data = client.EndReceive(Response, ref localEndpoint).Skip(6).ToArray();
                string[] serverinfos = Encoding.ASCII.GetString(data).Split(new char[] { '\0' }, 5);
                // getting and sanitizing map name
                string mapname = serverinfos[1].Split('.')[0].Replace("workshop/", "");
                // getting playerount
                int playercount = (int)Encoding.ASCII.GetBytes(serverinfos[4]).Skip(2).ToArray()[0];
                client.Close();
                client.Dispose();
                return new Tuple<string, int>(mapname, playercount);
            }
            else
            {
                client.Close();
                client.Dispose();
                return null;
            }
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
                Tuple<string, int> status = ServerQuery(server.serverIP);
                if (status != null)
                {
                    server.currentMap = status.Item1;
                    server.playerCount = status.Item2;
                    return server.tag + " server is playing " + server.currentMap + " with " + server.playerCount + " players";
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
                        activeServers += server.tag + " server is playing " +server.currentMap+ " with "+server.playerCount+" players. ";

                return activeServers.Equals(string.Empty) ? "no server is currently active" : activeServers;
            }
        }
    }
}
