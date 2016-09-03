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
        }

        public abstract void ProcessChatRoomMessage(ChatRoomIdentifier chatroomidentifier, UserIdentifier useridentifier, string Message);
        public abstract void ProcessPrivateMessage(UserIdentifier useridentifier, string Message);

        /// <summary>
        /// This Void Runs when the Bot has successfully logged into steam and is ready to interact
        /// </summary>
        public abstract void OnLoginCompleted();

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
