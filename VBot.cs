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
        public VBot(Bot bot) : base(bot){}
        public override void OnMessage(SteamFriends.FriendMsgCallback ChatMsg) { }
        public override void OnChatRoomMessage(SteamFriends.ChatMsgCallback ChatMsg) { }
        public override void OnLoginCompleted(){}
    }
}
