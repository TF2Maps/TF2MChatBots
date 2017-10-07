using System.Collections.Generic;

namespace SteamBotLite
{
    public partial class AdminModule
    {
        private class CommandListRetrieve : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public CommandListRetrieve(ModuleHandler bot, AdminModule module) : base(bot, "!CommandList")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string buildresponse = "";

                foreach (KeyValuePair<string, List<string[]>> item in module.CommandListHeld)
                {
                    buildresponse += System.Environment.NewLine;
                    buildresponse += item.Key;
                    buildresponse += System.Environment.NewLine;
                    foreach (string[] entry in item.Value)
                    {
                        buildresponse += "    " + entry[1] + " (" + entry[0] + " )" + System.Environment.NewLine; ;
                    }
                }
                userhandler.SendPrivateMessageProcessEvent(new MessageEventArgs(null) { Destination = Msg.Sender, ReplyMessage = buildresponse });
                return "Sent command list as private message!";
            }
        }
    }
}