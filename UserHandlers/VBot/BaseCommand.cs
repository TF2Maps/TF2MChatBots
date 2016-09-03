using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2.Internal;
using SteamKit2;

namespace SteamBotLite
{
    abstract class BaseCommand
    {
        public String command { get; protected set; }
        protected VBot userhandler;
        
        public BaseCommand(VBot bot, string command)
        {
            this.userhandler = bot;
            this.command = command;
        }
        public string run(UserIdentifier sender, string message = "")
        {
            string param = "";
            string[] command = message.Split(new char[] { ' ' }, 2);
            if (command.Length > 1)
                param = command[1].Trim();
            return exec(sender, param);
        }
        protected virtual string exec(UserIdentifier sender, string param)
        {
            return null;
        }
    }
}
