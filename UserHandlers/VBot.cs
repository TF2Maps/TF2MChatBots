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
        public VBot(SteamUser.LogOnDetails LogonDetails, SteamConnectionHandler SteamConnectionHandler) : base(LogonDetails,SteamConnectionHandler)
        {
            Console.WriteLine("Vbot Initialised");

        }
        public override void OnLoginCompleted() { }

        public override void OnMessage(SteamFriends.FriendMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            Console.WriteLine("MESSAGE RECEIVED");
            Console.WriteLine(ChatMsg.Message);
          //  OnMessage(ChatMsg.Message, ChatMsg.EntryType);       
        }
        public override void OnChatRoomMessage(SteamFriends.ChatMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            OnChatRoomMessage(ChatMsg.ChatRoomID, ChatMsg.ChatterID, ChatMsg.Message);
        }
        
        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        { }
        public override void OnMessage(string message, SteamKit2.EChatEntryType type)
        { }
        
    }
}
