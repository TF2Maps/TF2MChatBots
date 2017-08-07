using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    public class ServerList
    {
        ServerTrackingModule ServerTrackingModule;
        List<ServerInfo> ServerListObject { get; }

        public ServerList(ServerTrackingModule module, List<ServerInfo> serverlist)
        {
            ServerTrackingModule = module;
            ServerListObject = serverlist;
        }
        public void Add(ServerInfo server)
        {
            ServerListObject.Add(server);
            ServerTrackingModule.savePersistentData();
        }

        public void Remove(ServerInfo server)
        {
            ServerListObject.Remove(server);
            ServerTrackingModule.savePersistentData();
        }
        public void Clear()
        {
            ServerListObject.Clear();
            ServerTrackingModule.savePersistentData();
        }

        public IReadOnlyList<ServerInfo> Servers
        {
            get
            {
                return ServerListObject.AsReadOnly();
            }
        }

        public int Count()
        {
            return ServerListObject.Count;
        }

        public IEnumerator<ServerInfo> GetEnumerator()
        {
            return ServerListObject.GetEnumerator();
        }

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
}
