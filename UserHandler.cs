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
        public UserHandler(Bot bot )
        {
            Bot = bot;
            //OtherSID = sid;
        }
       // public SteamID OtherSID { get; private set; }
        public Bot Bot { get; private set; }
        public abstract void OnMessage(SteamFriends.FriendMsgCallback msg);
        public abstract void OnChatRoomMessage(SteamFriends.ChatMsgCallback msg);
        public abstract void OnLoginCompleted();
    }
}
