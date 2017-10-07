using System.Collections.Generic;
using System.Diagnostics;

namespace SteamBotLite
{
    public class AdminModule : BaseModule, OnLoginCompletedListiners
    {
        private List<string[]> CommandList;
        private Dictionary<string, List<string[]>> CommandListHeld;
        private ModuleHandler modulehandler;
        private UserHandler userhandler;

        public AdminModule(ModuleHandler handler, UserHandler userhandler, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(handler, Jsconfig)
        {
            DeletableModule = false;
            this.modulehandler = handler;
            this.userhandler = userhandler;

            loadPersistentData();
            savePersistentData();

            handler.AddLoginEventListiner(this);
            commands.Add(new CommandListRetrieve(handler, this));
            commands.Add(new UserInfo(handler, this));

            adminCommands.Add(new GetAllModules(handler, this));

            adminCommands.Add(new Reboot(handler, this));
            adminCommands.Add(new RunScript(handler, this));
            adminCommands.Add(new Rejoin(userhandler, modulehandler, this));
        }

        public override string getPersistentData()
        {
            return null;
        }

        public override void loadPersistentData()
        {
        }

        public override void OnAllModulesLoaded()
        {
            CommandListHeld = new Dictionary<string, List<string[]>>();
            foreach (BaseModule module in modulehandler.GetAllModules())
            {
                string[] HeaderNames = { "Command Type", "Command Name" };

                List<string[]> CommandList = new List<string[]>();

                foreach (BaseCommand command in module.commands)
                {
                    foreach (string entry in command.GetCommmand())
                    {
                        CommandList.Add(new string[] { "User Command", entry });
                    }
                }

                foreach (BaseCommand command in module.adminCommands)
                {
                    foreach (string entry in command.GetCommmand())
                    {
                        CommandList.Add(new string[] { "Admin Command", entry });
                    }
                }
                CommandListHeld.Add(module.GetType().Name.ToString(), CommandList);

                modulehandler.HTMLFileFromArray(HeaderNames, CommandList, module.GetType().Name.ToString());
            }
        }

        public void OnLoginCompleted()
        {
        }

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

        private class GetAllModules : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            private ModuleHandler modulehandler;

            public GetAllModules(ModuleHandler bot, AdminModule module) : base(bot, "!ModuleList")
            {
                this.module = module;
                this.modulehandler = bot;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string Response = "";

                foreach (BaseModule ModuleEntry in modulehandler.GetAllModules())
                {
                    Response += ModuleEntry.GetType().Name.ToString() + " ";
                }

                return Response;
            }
        }

        private class Reboot : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public Reboot(ModuleHandler bot, AdminModule module) : base(bot, "!Reboot")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.userhandler.Reboot();
                return "Rebooted";
            }
        }

        private class Rejoin : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public Rejoin(UserHandler bot, ModuleHandler modulehandler, AdminModule module) : base(modulehandler, "!Rejoin")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.userhandler.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.LeaveChat);
                module.userhandler.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.EnterChat);
                return "Rejoined!";
            }
        }

        private class RunScript : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public RunScript(ModuleHandler bot, AdminModule module) : base(bot, "!RunUpdateScript")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "../../update.sh"
                    }
                };

                proc.Start();

                Process.GetCurrentProcess().Kill();
                return "Script should've ran";
            }
        }

        private class UserInfo : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public UserInfo(ModuleHandler bot, AdminModule module) : base(bot, "!UserInfo")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return string.Format("Your ID is: {0} | {1} | {2}", Msg.Sender.identifier, Msg.Sender.identifier.ToString(), Msg.Sender.DisplayName);
            }
        }
    }
}