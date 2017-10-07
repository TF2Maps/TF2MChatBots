using System;

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
}