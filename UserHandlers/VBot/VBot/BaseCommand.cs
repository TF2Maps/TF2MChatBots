using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2.Internal;
using SteamKit2;

namespace SteamBotLite
{
    public abstract class BaseCommand
    {
        public String command { get; protected set; }
        protected ModuleHandler userhandler;
        
        public BaseCommand(ModuleHandler bot, string command)
        {
            this.userhandler = bot;
            this.command = command;
        }
        public string run(MessageEventArgs Msg, string message = "" )
        {
            string param = "";
            string[] command = message.Split(new char[] { ' ' }, 2);
            if (command.Length > 1)
            {
                param = command[1].Trim();
            }
            else
            {
                param = command[0].Trim();
            }

            return exec(Msg, param);
        }

        public virtual bool CheckCommandExists(MessageEventArgs Msg, string Message)
        {
            if (Message.StartsWith(command, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public virtual string[] GetCommmand ()
        {
            return new string[] { command };
        }
    

        protected virtual string exec(MessageEventArgs Msg, string param)
        {
            return null;
        }
    }
}
