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
        /// <summary>
        /// Our Logon Details, which may be useful later
        /// </summary>
        public SteamUser.LogOnDetails LogonDetails;
        /// <summary>
        /// The SteamConnectionhandler that will act as the bridge between Steam and the Userhandler
        /// </summary>
        public SteamConnectionHandler steamConnectionHandler { get; set; }

        /// <summary>
        /// Sets the SteamConnectionHandler to Bot
        /// </summary>
        /// <param name="SteamConnectionHandler"></param>
        public UserHandler(SteamConnectionHandler SteamConnectionHandler)
        {
            steamConnectionHandler = SteamConnectionHandler;
        }
        
        public abstract void OnMessage(SteamFriends.FriendMsgCallback msg);
        public abstract void OnChatRoomMessage(SteamFriends.ChatMsgCallback msg);
        /// <summary>
        /// This Void Runs when the Bot has successfully logged into steam and is ready to interact
        /// </summary>
        public abstract void OnLoginCompleted();
        
    }
}
