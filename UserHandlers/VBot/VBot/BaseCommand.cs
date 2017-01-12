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
        public string run(MessageEventArgs Msg, string message = "")
        {
            string param = "";
            string[] command = message.Split(new char[] { ' ' }, 2);
            if (command.Length > 1)
                param = command[1].Trim();
            return exec(Msg, param);
        }

        public virtual bool CheckCommand(MessageEventArgs Msg, string Message)
        {
            if (Message.StartsWith(command, StringComparison.OrdinalIgnoreCase))
            {
                return run(Msg, Message);
            }
            else
            {
                return null;
            }
        }

        protected virtual string[] GetCommmand ()
        {
            return new string[] { command };
        }
    

        protected virtual string exec(MessageEventArgs Msg, string param)
        {
            return null;
        }
    }
}
