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

        public override void ProcessChatRoomMessage(object sender, MessageEventArgs e)
        {
            CrashCheck = 0;
            Tick.Stop();
            Tick.Start();
            Console.WriteLine("Restarted the timer");
        }

        public override void ProcessPrivateMessage(object sender, MessageEventArgs e) { }
        

        public override void OnLoginCompleted(object sender, EventArgs e) { }
        

        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e)
        {
            MessageEventArgs msg = new MessageEventArgs(e.Item1.Application);
            msg.Destination = new ChatroomEntity(e.Item1.ParentIdentifier, e.Item1.Application);
            msg.ReplyMessage = "Hi, the TF2Maps community has made the change over to a much more active Discord server. The Steam group will still be used for announcements and events, but you can find help, advice, and a thriving and friendly community over on our Discord at https://discord.gg/D5dMfb7 We hope to see you there!";
            SendChatRoomMessageProcessEvent(msg);
            CrashCheck = 0;
            Tick.Stop();
            Tick.Start();
            Console.WriteLine("Restarted the timer");
        }
    }
}
