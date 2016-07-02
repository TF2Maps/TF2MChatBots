using System;
using SteamKit2;
using System.Timers;

namespace SteamBotLite
{
    class VBot : UserHandler
    {
        SteamID ChatRoomID = 103582791429594873;
        double interval = 5000;
        int GhostCheck = 120;
        Timer Tick;

        public VBot(SteamConnectionHandler SteamConnectionHandler) : base(SteamConnectionHandler)
        {
            Console.WriteLine("Vbot Initialised");
        }

        public override void OnLoginCompleted()
        {
            steamConnectionHandler.SteamFriends.SetPersonaName("V2Bot");
            steamConnectionHandler.SteamFriends.JoinChat(103582791429594873);
            InitTimer();
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
            if (ChatMsg.Message == "Reboot")
                Reboot();
        
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

    }
}
