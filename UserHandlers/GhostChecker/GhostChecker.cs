using System;
using System.Timers;

namespace SteamBotLite
{
    public class GhostChecker :UserHandler
    {
        double interval = 60000;
        readonly int InitialGhostCheck = 10;
        int GhostCheck = 480;
        int CrashCheck = 0;
        Timer Tick;

        public GhostChecker()
        {
            InitTimer();
        }
        /// <summary>
        /// The Main Timer's method, executed per tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TickTasks(object sender, EventArgs e)
        {
            GhostCheck--;
            Console.WriteLine(string.Format("Ghostcheck = {0}"), GhostCheck);
            if (GhostCheck <= 1)
            {
                GhostCheck = InitialGhostCheck;
                CrashCheck += 1;
                FireMainChatRoomEvent(ChatroomEventEnum.LeaveChat);
                FireMainChatRoomEvent(ChatroomEventEnum.EnterChat);
            }
            if (CrashCheck >= 4)
            {
                CrashCheck = 0;
                Reboot();
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

        public override void ProcessChatRoomMessage(object sender, MessageProcessEventData e)
        {
            GhostCheck = InitialGhostCheck;
            CrashCheck = 0;
        }

        public override void ProcessPrivateMessage(object sender, MessageProcessEventData e) { }
        

        public override void OnLoginCompleted(object sender, EventArgs e) { }
        

        public override void ChatMemberInfo(UserIdentifier useridentifier, bool MemberInfo) { }
    }
}
