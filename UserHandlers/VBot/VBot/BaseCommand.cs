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

            param = command[0].Trim();

            if (command.Length > 1)
            {
                param = command[1].Trim();
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

        public static string RemoveWhiteSpacesFromString (string param)
        {
            string[] command = param.Split(new char[] { ' ' });

            string returnstring = "";

            for (int i = 0; i < command.Length; i++)
            {
                returnstring += command[i].Trim();

                if (command[i].Trim().Length > 0 & i + 1 != command.Length) //We add an extra space if the string isn't empty and not the last string
                {
                    returnstring += " ";
                }
            }

            return returnstring;

        }
    }
}
