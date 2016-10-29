using System;
using System.Timers;

namespace SteamBotLite
{
    public class GhostChecker :UserHandler
    {
        
       // double interval = 300000; //Five Minutes
        readonly double Minutes = 10;
        double interval;
        enum GhostStatus { Chatghosted, ChatPotentiallyGhosted, ChatHasNotGhosted , ChatCrashed};
        GhostStatus CurrentGhostStatus = GhostStatus.ChatHasNotGhosted;
        int CrashCheck = 0;
        readonly int MinutesThreshhole = 10;
        int timeleft;
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
            FireMainChatRoomEvent(ChatroomEventEnum.LeaveChat);
            FireMainChatRoomEvent(ChatroomEventEnum.EnterChat);
            CrashCheck++;
            
            
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
            interval = Minutes * 1000 * 60;
            Tick = new Timer();
            Tick.Elapsed += new ElapsedEventHandler(TickTasks);
            Tick.Interval = interval; // in miliseconds
            Tick.Start();
        }

        public override void ProcessChatRoomMessage(object sender, MessageProcessEventData e)
        {
            CrashCheck = 0;
            Tick.Stop();
            Tick.Start();
        }

        public override void ProcessPrivateMessage(object sender, MessageProcessEventData e) { }
        

        public override void OnLoginCompleted(object sender, EventArgs e) { }
        

        public override void ChatMemberInfo(UserIdentifier useridentifier, bool MemberInfo) { }
    }
}
