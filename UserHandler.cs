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
        public SteamUser.LogOnDetails LogonDetails;

        UserHandler(SteamConnectionHandler bot )
        {
            Bot = bot;
        }
        public UserHandler (SteamUser.LogOnDetails LogonDetailsTransfer)
        {
            LogonDetails = LogonDetailsTransfer;
        }
        SteamConnectionHandler Bot { get; set; }
        public abstract void OnMessage(SteamFriends.FriendMsgCallback msg);
        public abstract void OnChatRoomMessage(SteamFriends.ChatMsgCallback msg);
        public abstract void OnLoginCompleted();
        public abstract void OnChatRoomMessage(SteamID chatID, SteamID sender, string message); //Incase older methods rely heavily on these exact parameters, these are left in, and can be re-routed into
        public abstract void OnMessage(string message, SteamKit2.EChatEntryType type); //Incase older methods rely heavily on these exact parameters, these are left in, and can be re-routed into

    }
}
