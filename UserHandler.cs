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
        /// The SteamConnectionhandler that will act as the bridge between Steam and the Userhandler
        /// </summary>
        public ApplicationInterface appinterface { get; set; }

        /// <summary>
        /// Sets the SteamConnectionHandler to Bot
        /// </summary>
        /// <param name="SteamConnectionHandler"></param>
        public UserHandler(ApplicationInterface SteamConnectionHandler)
        {
            appinterface = SteamConnectionHandler;
            appinterface.AnnounceLoginCompletedEvent += OnLoginCompleted;
            appinterface.PrivateMessageEvent += ProcessPrivateMessage;
            appinterface.ChatRoomMessageEvent += ProcessChatRoomMessage;
        }

        public abstract void ProcessChatRoomMessage(object sender, MessageProcessEventData e);

        public abstract void ProcessPrivateMessage(object sender, MessageProcessEventData e);

        public abstract void OnLoginCompleted(object sender, EventArgs e);

        
        public abstract void ChatMemberInfo(UserIdentifier useridentifier, bool MemberInfo); //TODO make this an object, not a bool


        /// <summary>
        /// Reboot the connection with steam
        /// </summary>
        public void Reboot()
        {
            appinterface.Reboot();
        }
    }
}
