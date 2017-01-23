using Newtonsoft.Json;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SteamBotLite
{
    class CountDownModule : MotdModule
    {
        public CountDownModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig){ }

        public override string GetName()
        {
            return "Countdown";
        }
        public override int DefaultPostCountLimit()
        {
            return 24 * 60;
        }
        /// <summary>
        /// Posts the MOTD to the group chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public override void MotdPost(object sender, EventArgs e)
        {
            if (message != null && !message.Equals(string.Empty))
            {
                double hoursdata = (postCountLimit - postCount) / 60;
                Math.Ceiling(hoursdata);

                string hours = (hoursdata.ToString().PadLeft(2, '0'));
                string minutes = ((postCountLimit - postCount) % 60).ToString().PadLeft(2, '0');
                string StatusMessage = hours + ":" + minutes + " " + base.message;
                
                userhandler.SetStatusmessageEvent(StatusMessage);
                
                postCount++;
                if (postCount > postCountLimit)
                    message = null;
                savePersistentData();
            }
        }


    }
}