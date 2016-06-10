using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamBotLite
{
    class VBot : UserHandler
    {
        public VBot(Bot bot, SteamID sid) : base(bot, sid){}
        public override void OnMessage(string message, SteamKit2.EChatEntryType type) { }
        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message) { }
        public override void OnLoginCompleted(){}
    }
}
