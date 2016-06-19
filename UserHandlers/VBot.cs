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
        public VBot(SteamConnectionHandler SteamConnectionHandler) : base(SteamConnectionHandler)
        {
            Console.WriteLine("Vbot Initialised");
        }

        public override void OnLoginCompleted()
        {
            steamConnectionHandler.SteamFriends.SetPersonaName("V2Bot");
            steamConnectionHandler.SteamFriends.JoinChat(103582791429594873);

            Console.WriteLine("{0} User: {1} Is now online", steamConnectionHandler.ID, steamConnectionHandler.LoginData.Username); //Lets tell the User we're now online
        }

        public override void OnMessage(SteamFriends.FriendMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            Console.WriteLine("MESSAGE RECEIVED");
            Console.WriteLine(ChatMsg.Message);
          //  OnMessage(ChatMsg.Message, ChatMsg.EntryType);       
        }
        public override void OnChatRoomMessage(SteamFriends.ChatMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            Console.WriteLine("{0}:{1}", steamConnectionHandler.SteamFriends.GetFriendPersonaName(ChatMsg.ChatterID), ChatMsg.Message);
            steamConnectionHandler.SteamFriends.GetFriendPersonaState(103582791429594873);
        }
        
    }
}
