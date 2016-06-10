using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamBotLite
{
    public abstract class UserHandler
    {
        public UserHandler(Bot bot, SteamID sid)
        {
            Bot = bot;
            OtherSID = sid;
        }
        public SteamID OtherSID { get; private set; }
        public Bot Bot { get; private set; }
        public abstract void OnMessage(string message, SteamKit2.EChatEntryType type);
        public abstract void OnChatRoomMessage(SteamID chatID, SteamID sender, string message);
        public abstract void OnLoginCompleted();
    }
}
