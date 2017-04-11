using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace SteamBotLite
{
    public class AdminModule : BaseModule , OnLoginCompletedListiners
    {

        UserHandler userhandler;
        ModuleHandler modulehandler;

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

            adminCommands.Add(new RemoveModule(handler, this));
            adminCommands.Add(new AddModule(handler, this));
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
        Dictionary<string, List<string[]>> CommandListHeld;
        List<string[]> CommandList;

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
                CommandListHeld.Add(module.GetType().Name.ToString()  , CommandList);

                modulehandler.HTMLFileFromArray(HeaderNames, CommandList, module.GetType().Name.ToString());
            }
           
        }

        public void OnLoginCompleted()
        {
        }

        private class CommandListRetrieve : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;

            public CommandListRetrieve(ModuleHandler bot, AdminModule module) : base(bot, "!CommandList")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string buildresponse = "";


                foreach(KeyValuePair<string,List<string[]>> item in module.CommandListHeld)
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
        private class UserInfo : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;

            public UserInfo(ModuleHandler bot, AdminModule module) : base(bot, "!UserInfo")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                return string.Format("Your ID is {0} | {1} | {2}" ,Msg.Sender.identifier, Msg.Sender.identifier.ToString(),Msg.Sender.DisplayName );
            }
        }

        private class Reboot : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            
            public Reboot(ModuleHandler bot, AdminModule module) : base(bot, "!Reboot")  {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)  {
                module.userhandler.Reboot();
                return "Rebooted";
            }
        }

        private class RunScript : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;

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

                return "Script should've ran";
            }
        }

       
        private class Rejoin : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;

            public Rejoin(UserHandler bot, ModuleHandler modulehandler , AdminModule module) : base(modulehandler, "!Rejoin")
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

        private class RemoveModule : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            ModuleHandler ModuleHandler;

            public RemoveModule(ModuleHandler bot, AdminModule module) : base(bot, "!ModuleRemove")
            {
                this.module = module;
                ModuleHandler = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                ModuleHandler.Disablemodule(param);
                return "Removing Module...";
            }

        }

        private class AddModule : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            ModuleHandler modulehandler;

            public AddModule(ModuleHandler bot, AdminModule module) : base(bot, "!ModuleAdd")
            {
                this.module = module;
                modulehandler = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                modulehandler.Enablemodule(param);
                return "Adding Module...";
            }

        }

        private class GetAllModules : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            ModuleHandler modulehandler;

            public GetAllModules(ModuleHandler bot, AdminModule module) : base(bot, "!ModuleList")
            {
                this.module = module;
                this.modulehandler = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string Response = "";

                foreach (BaseModule ModuleEntry in modulehandler.GetAllModules()) {
                    Response += ModuleEntry.GetType().Name.ToString() + " ";
                }

                return Response;
            }

        }
    }
}