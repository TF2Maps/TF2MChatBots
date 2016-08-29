using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SteamBotLite
{
    class BaseTask
    {
        Timer timer;
        public BaseTask(int delay, ElapsedEventHandler e)
        {
            timer = new Timer(delay);
            timer.Elapsed += e;
            timer.Start();
        }
    }
}
