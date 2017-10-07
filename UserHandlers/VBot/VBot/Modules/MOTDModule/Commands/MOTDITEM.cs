using System.Timers;

namespace SteamBotLite
{
    internal partial class MotdModule
    {
        private class MOTDITEM
        {
            public string message;
            private BaseTask Task;

            public MOTDITEM(int updateinterval, ElapsedEventHandler PostMethod)
            {
                Task = new BaseTask(updateinterval, new System.Timers.ElapsedEventHandler(PostMethod));
            }

            public int postCount { get; set; }
            public int postCountLimit { get; set; }
        }
    }
}