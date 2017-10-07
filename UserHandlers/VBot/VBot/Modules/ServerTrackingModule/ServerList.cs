using System;
using System.Collections.Generic;

namespace SteamBotLite
{
    public class TrackingServerList
    {
        private ServerTrackingModule ServerTrackingModule;
        private TrackingServerList trackedServers;

        private List<TrackingServerInfo> TrackingServerListObject { get; }

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

        public void Add(TrackingServerInfo server)
        {
            TrackingServerListObject.Add(server);
            ServerTrackingModule.savePersistentData();
        }

        public void Remove(TrackingServerInfo server)
        {
            TrackingServerListObject.Remove(server);
            ServerTrackingModule.savePersistentData();
        }

        public void Clear()
        {
            TrackingServerListObject.Clear();
            ServerTrackingModule.savePersistentData();
        }

        public IReadOnlyList<TrackingServerInfo> Servers
        {
            get
            {
                return TrackingServerListObject.AsReadOnly();
            }
        }

        public int Count()
        {
            return TrackingServerListObject.Count;
        }

        public IEnumerator<TrackingServerInfo> GetEnumerator()
        {
            return TrackingServerListObject.GetEnumerator();
        }
    }

    public class TrackingServerInfo : EventArgs
    {
        public string serverIP;
        public int port;

        public string tag;
        public int playerCount;
        public int capacity;
        public string currentMap = "";

        public TrackingServerInfo(string serverIP, int port, string tag)
        {
            this.serverIP = serverIP;
            this.port = port;
            this.tag = tag;
        }

        public void update(TrackingServerInfo updated)
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
}