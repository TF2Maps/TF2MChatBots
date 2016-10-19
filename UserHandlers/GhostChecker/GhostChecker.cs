using System;
using System.Timers;

namespace SteamBotLite
{
    public class GhostChecker :UserHandler
    {
        
        double interval = 300000; //Five Minutes
        enum GhostStatus { Chatghosted, ChatPotentiallyGhosted, ChatHasNotGhosted , ChatCrashed};
        GhostStatus CurrentGhostStatus = GhostStatus.ChatHasNotGhosted;
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
            switch (CurrentGhostStatus)
            {
                case (GhostStatus.Chatghosted):
                    FireMainChatRoomEvent(ChatroomEventEnum.LeaveChat);
                    FireMainChatRoomEvent(ChatroomEventEnum.EnterChat);
                    CrashCheck++;
                    InitTimer();
                    Console.WriteLine("Chat Ghosted, Crash check is at: {0}", CrashCheck);
                    CurrentGhostStatus = GhostStatus.ChatHasNotGhosted;
                    break;
                case (GhostStatus.ChatPotentiallyGhosted):
                    CurrentGhostStatus = GhostStatus.Chatghosted;
                    break;
                case (GhostStatus.ChatHasNotGhosted):
                    CurrentGhostStatus = GhostStatus.ChatPotentiallyGhosted; //Therefore if this method runs twice (10 minutes) we have a ghost
                    break;
                case (GhostStatus.ChatCrashed):
                    break;
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
            CurrentGhostStatus = GhostStatus.ChatHasNotGhosted;
            CrashCheck = 0;
        }

        public override void ProcessPrivateMessage(object sender, MessageProcessEventData e) { }
        

        public override void OnLoginCompleted(object sender, EventArgs e) { }
        

        public override void ChatMemberInfo(UserIdentifier useridentifier, bool MemberInfo) { }
    }
}
