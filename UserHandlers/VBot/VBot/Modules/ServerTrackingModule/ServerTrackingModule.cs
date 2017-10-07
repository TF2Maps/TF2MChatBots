using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace SteamBotLite
{
    public partial class ServerTrackingModule : BaseModule
    {
        public ModuleHandler Bot;

        public TrackingServerList TrackedServers;

        //For Testing Purposes
        private TrackingServerInfo EmulatedTrackingServerInfo;

        private bool IsEmulating = false;

        private BaseTask serverUpdate;

        private HTMLFileFromArrayListiners WebServer;

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
                    commands.Add(new ServerStatus(bot, ServersBeingTracked[i], this));
                }
            }
            else
            {
                Console.WriteLine("No servers found in config file");
                servers = null;
            }

            TrackedServers = new TrackingServerList(this, ServersBeingTracked);

            commands.Add(new ActiveServers(bot, this));
            commands.Add(new SpecificServerStatus(bot, this));
            adminCommands.Add(new ServerAdd(bot, this));
            adminCommands.Add(new ServerRemove(bot, this));
            adminCommands.Add(new FullServerQuery(bot, this));

            serverUpdate = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(SyncTrackingServerInfo));

            ServerMapChanged += bot.ServerUpdated;
            ServerMapChanged += ServerTrackingModule_ServerMapChanged;
        }

        public event EventHandler<TrackingServerInfo> ServerMapChanged;

        public void EmulateServerQuery(TrackingServerInfo Response)
        {
            EmulatedTrackingServerInfo = Response;
            IsEmulating = true;
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

        public string NameToserverCommand(string servername)
        {
            return "!" + servername.ToLower() + "server";
        }

        public override void OnAllModulesLoaded()
        {
        }

        // queries a server and returns a <string, int> Tuple (filename, playercount)
        public TrackingServerInfo ServerQuery(TrackingServerInfo server)
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

            if (e.playerCount > 8)
            {
                userhandler.BroadcastMessageProcessEvent(e.ToString());

                TableDataValue HeaderServer = new TableDataValue();
                HeaderServer.VisibleValue = "Server";

                TableDataValue ServerLabel = new TableDataValue();
                ServerLabel.VisibleValue = e.tag;
                ServerLabel.Link = "steam://connect/" + e.serverIP + ":" + e.port;

                string RecentlyTestedTableLabel = "Recently Tested";
                WebServer.SetTableHeader(RecentlyTestedTableLabel, new TableDataValue[] { HeaderName, HeaderNamePlayerCount, HeaderTime, HeaderServer });
                WebServer.AddEntryWithLimit(RecentlyTestedTableLabel, new TableDataValue[] { MapName, PlayerCount, Time, ServerLabel }, 10);
            }
        }
    }
}