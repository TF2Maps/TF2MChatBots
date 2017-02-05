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
    public class ServerModule : BaseModule
    {
        public event EventHandler<ServerInfo> ServerMapChanged;


        private BaseTask serverUpdate;

        public ServerList serverList;

        public ModuleHandler Bot;

        

        public override void OnAllModulesLoaded() { }


        public ServerModule(ModuleHandler bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {

            Bot = bot;
            List<ServerInfo> ServerList = new List<ServerInfo>();

            int updateInterval = int.Parse(config["updateInterval"].ToString());
            Tuple<string, string, int>[] servers;

            if (File.Exists(ModuleSavedDataFilePath()))
            {
                ServerList = JsonConvert.DeserializeObject<List<ServerInfo>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                for (int i = 0; i < ServerList.Count; i++)
                {
                    commands.Add(new Status(bot, ServerList[i], this));
                }
            }
            else
            {
                Console.WriteLine("No servers found in config file");
                servers = null;
            }

            serverList = new ServerList(this, ServerList);
            commands.Add(new Active(bot, this));
            commands.Add(new SpecificServerStatus(bot, this));
            adminCommands.Add(new ServerAdd(bot, this));
            adminCommands.Add(new ServerRemove(bot, this));
            adminCommands.Add(new FullServerQuery(bot, this));

            serverUpdate = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(SyncServerInfo));

            ServerMapChanged += bot.ServerUpdated;
            ServerMapChanged += ServerModule_ServerMapChanged;
        }

        public string NameToserverCommand (string servername)
        {
            return "!" + servername.ToLower() + "server";
        }

        private void ServerModule_ServerMapChanged(object sender, ServerInfo e)
        {
            if (e.playerCount > 8)
            {
                userhandler.BroadcastMessageProcessEvent(e.ToString());
            }
        }

        

        public void SyncServerInfo(object sender, EventArgs e)
        {
            for (int x = 0; x < serverList.Servers.Count; x++)
            {
                ServerInfo currentserverstate = ServerQuery(serverList.Servers[x]);

                if (currentserverstate != null)
                {
                    if (!(serverList.Servers[x].currentMap.Equals(currentserverstate.currentMap)))
                    {
                        ServerMapChanged(this, currentserverstate);
                    }
                    serverList.Servers[x].update(currentserverstate);
                }
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

                foreach (Tuple<string, string, int> servconf in servers)
                {
                    IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse(servconf.Item2), servconf.Item3);
                    ServerInfo serverInfo = new ServerInfo(servconf.Item2, servconf.Item3, servconf.Item1);
                    serverList.Add(serverInfo);
                }
            }
            catch
            {
                Console.WriteLine("There was an error loading the serverlist");
            }
        }

        
        // queries a server and returns a <string, int> Tuple (filename, playercount)
        public  ServerInfo ServerQuery(ServerInfo server)
        {
            if (IsEmulating) //For testing purposes, we can assign and set an emulated serverinfo response
            {
                return EmulatedServerInfo;
            }
            else
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
        private ServerInfo EmulatedServerInfo;
        private bool IsEmulating = false;

        public void EmulateServerQuery(ServerInfo Response)
        {
            EmulatedServerInfo = Response;
            IsEmulating = true;
        }

        // Special status commands

        private sealed class Status : BaseCommand
        {
            // Automaticaly generated status command for each server under the config
            ServerInfo server;
            ServerModule servermodule;

            public Status(ModuleHandler bot, ServerInfo server, ServerModule module) : base(bot, module.NameToserverCommand(server.tag))
            {
                this.server = server;
                servermodule = module;

            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                ServerInfo status = servermodule.ServerQuery(server);
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
            ServerModule servermodule;

            public SpecificServerStatus(ModuleHandler bot, ServerModule module) : base(bot, "")
            {
                servermodule = module;

            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                foreach (ServerInfo server in servermodule.serverList)
                {
                    if (server.tag.Equals(param))
                    {
                        return servermodule.ServerQuery(server).ToString();
                    }
                }
                return "An error occured when querying the server!";
            }

            public override bool CheckCommandExists(MessageEventArgs Msg, string Message)
            {
                foreach (ServerInfo server in servermodule.serverList)
                {
                    if (Message.StartsWith(servermodule.NameToserverCommand(server.tag), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }

                }
                return false;
            }
            
            public override string[] GetCommmand()
            {
                int AmountOfServers = servermodule.serverList.Count();
                string[] ServerList = new string[AmountOfServers];

                for (int x = 0; x < AmountOfServers; x++ )
                {
                    ServerList[AmountOfServers] = servermodule.NameToserverCommand(servermodule.serverList.Servers[x].tag);
                }

                return ServerList;
            }
        }

        private class ServerAdd : BaseCommand
        {
            ServerModule module;
            public ServerAdd(ModuleHandler bot, ServerModule module) : base(bot, "!serveradd")
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

            public ServerRemove(ModuleHandler bot, ServerModule module) : base(bot, "!serverremove")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
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
            public Active(ModuleHandler bot, ServerModule module) : base(bot, "!Active")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
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
            public FullServerQuery(ModuleHandler bot, ServerModule module) : base(bot, "!Serverquery")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
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
