using System;
using System.Collections.Generic;

namespace SteamBotLite
{
    public class TrackingServerInfo : EventArgs
    {
        public int capacity;
        public string currentMap = "";
        public int playerCount;
        public int port;
        public string serverIP;
        public string tag;

        public TrackingServerInfo(string serverIP, int port, string tag)
        {
            this.serverIP = serverIP;
            this.port = port;
            this.tag = tag;
        }

        public override string ToString()
        {
            return this.tag + " server is now on " + this.currentMap +
                " - " + this.playerCount + "/" + this.capacity +
                " - join: steam://connect/" + this.serverIP + ":" + this.port;
        }

        public void update(TrackingServerInfo updated)
        {
            this.playerCount = updated.playerCount;
            this.capacity = updated.capacity;
            this.currentMap = updated.currentMap;
        }
    }

    public class TrackingServerList
    {
        private ServerTrackingModule ServerTrackingModule;
        private TrackingServerList trackedServers;

        public TrackingServerList(ServerTrackingModule module, List<TrackingServerInfo> TrackingServerList)
        {
            ServerTrackingModule = module;
            TrackingServerListObject = TrackingServerList;
        }

        public TrackingServerList(ServerTrackingModule serverTrackingModule, TrackingServerList trackedServers)
        {
            ServerTrackingModule = serverTrackingModule;
            this.trackedServers = trackedServers;
        }

        public IReadOnlyList<TrackingServerInfo> Servers
        {
            get
            {
                return TrackingServerListObject.AsReadOnly();
            }
        }

        private List<TrackingServerInfo> TrackingServerListObject { get; }

        public void Add(TrackingServerInfo server)
        {
            TrackingServerListObject.Add(server);
            ServerTrackingModule.savePersistentData();
        }

        public void Clear()
        {
            TrackingServerListObject.Clear();
            ServerTrackingModule.savePersistentData();
        }

        public int Count()
        {
            return TrackingServerListObject.Count;
        }

        public IEnumerator<TrackingServerInfo> GetEnumerator()
        {
            return TrackingServerListObject.GetEnumerator();
        }

        public void Remove(TrackingServerInfo server)
        {
            TrackingServerListObject.Remove(server);
            ServerTrackingModule.savePersistentData();
        }
    }
}