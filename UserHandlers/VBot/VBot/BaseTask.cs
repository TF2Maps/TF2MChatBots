using System.Timers;

namespace SteamBotLite
{
    public class BaseTask
    {
        private Timer timer;

        public BaseTask(int delay, ElapsedEventHandler e)
        {
            timer = new Timer(delay);
            timer.Elapsed += e;
            timer.Start();
        }
    }
}