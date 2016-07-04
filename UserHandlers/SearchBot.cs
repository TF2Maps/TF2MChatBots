using System;
using System.Collections.Generic;
using SteamKit2;
using System.IO;
using Newtonsoft.Json;
using System.Timers;

namespace SteamBotLite
{
    class SearchBot : UserHandler
    {
        SteamID ChatRoomID = 103582791429594873;
        double interval = 5000;
        int GhostCheck = 120;
        Timer Tick;
       
        List<SearchClassEntry> Searches;

        public SearchBot(SteamConnectionHandler SteamConnectionHandler) : base(SteamConnectionHandler)
        {
        }

        public override void ClanStateCallback(SteamFriends.ClanStateCallback callback)
        {
            Console.WriteLine("Members Chatting Callback: {0}",callback.MemberChattingCount);
            steamConnectionHandler.SteamFriends.SetPersonaName("[" + callback.MemberChattingCount + "]SearchBot");
        }

        public override void OnLoginCompleted()
        {
            Searches = JsonConvert.DeserializeObject<List<SearchClassEntry>>(File.ReadAllText(Path.Combine("searchbot", "searchdata.json")));
            steamConnectionHandler.SteamFriends.SetPersonaName("SearchBot");
            steamConnectionHandler.SteamFriends.JoinChat(ChatRoomID);
            InitTimer();
            Console.WriteLine("{0} User: {1} Is now online", steamConnectionHandler.ID, steamConnectionHandler.LoginData.Username); //Lets tell the User we're now online
        }

        public override void OnMessage(SteamFriends.FriendMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            string[] Words = ChatMsg.Message.Split(' ');
            Console.WriteLine("{0}:{1}", steamConnectionHandler.SteamFriends.GetFriendPersonaName(ChatMsg.Sender), ChatMsg.Message);
           /* foreach (SearchClassEntry Entry in Searches)
            {
               // if ((Words.Length > 0) & (string.Equals(Words[0], Entry.Command, StringComparison.OrdinalIgnoreCase)))
                    //steamConnectionHandler.SteamFriends.SendChatRoomMessage(ChatRoomID, EChatEntryType.ChatMsg, SearchClass.Search(Entry, ChatMsg.Message.Substring(Words[0].Length)));
            } */
        }
        public override void OnChatRoomMessage(SteamFriends.ChatMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            GhostCheck = 120;
            string[] Words = ChatMsg.Message.Split(' ');
            Console.WriteLine("{0}:{1}", steamConnectionHandler.SteamFriends.GetFriendPersonaName(ChatMsg.ChatterID), ChatMsg.Message);            
            foreach (SearchClassEntry Entry in Searches)
                {
                if ((Words.Length > 1) & (string.Equals(Words[0], Entry.Command, StringComparison.OrdinalIgnoreCase)))
                    steamConnectionHandler.SteamFriends.SendChatRoomMessage(ChatRoomID, EChatEntryType.ChatMsg, SearchClass.Search(Entry, ChatMsg.Message.Substring(Words[0].Length)));
            }
        }

        /// <summary>
        /// The Main Timer's method, executed per tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TickTasks(object sender, EventArgs e)
        {
            GhostCheck--;

            if (GhostCheck <= 1)
            {
                GhostCheck = 120;
                steamConnectionHandler.SteamFriends.LeaveChat(new SteamID(ChatRoomID));
                steamConnectionHandler.SteamFriends.JoinChat(new SteamID(ChatRoomID));
            }
        }

        /// <summary>
        /// Initialises the main timer
        /// </summary>
        void InitTimer()
        {
            Tick = new Timer();
            Tick.Elapsed += new ElapsedEventHandler(TickTasks);
            Tick.Interval = interval; // in miliseconds
            Tick.Start();
        }

        public override void ChatMemberInfo(SteamFriends.ChatMemberInfoCallback callback)
        {
        }
    }
}
