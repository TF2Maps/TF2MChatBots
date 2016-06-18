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
            Bot.SteamFriends.SetPersonaName("V2Bot");
            Bot.SteamFriends.JoinChat(103582791429594873);
        }

        public override void OnMessage(SteamFriends.FriendMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            Console.WriteLine("MESSAGE RECEIVED");
            Console.WriteLine(ChatMsg.Message);
          //  OnMessage(ChatMsg.Message, ChatMsg.EntryType);       
        }
        public override void OnChatRoomMessage(SteamFriends.ChatMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            Console.WriteLine("{0}:{1}", ChatMsg.ChatRoomID, ChatMsg.Message);
            Bot.SteamFriends.GetFriendPersonaState(103582791429594873);
        }
        public override void OnClanStateChange (SteamFriends.ClanStateCallback callback)
        {
            Console.WriteLine(callback.MemberChattingCount); 
        }
        
        public override void OnChatRoomMessage(SteamID chatID, SteamID sender, string message)
        { }
        public override void OnMessage(string message, SteamKit2.EChatEntryType type)
        { }
        
    }
}
