using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace SteamBotLite
{
    class AdminModule : BaseModule
    {
       
        VBot SteamBot;

        public AdminModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            DeletableModule = false;
            SteamBot = bot;
            adminCommands.Add(new Reboot(bot, this));
            adminCommands.Add(new Rename(bot, this));
            adminCommands.Add(new RemoveModule(bot, this));
            adminCommands.Add(new AddModule(bot, this));
            adminCommands.Add(new GetAllModules(bot, this));
            adminCommands.Add(new Rejoin(bot, this));
            
        }

        public override string getPersistentData()
        {
               return "";
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }

        public override void OnAllModulesLoaded()
        {
            foreach(BaseModule module in SteamBot.ModuleList)
            {
                
                string[] HeaderNames = { "Command Type", "Command Name" };

                List<string[]> CommandList = new List<string[]>();

                foreach (BaseCommand command in module.commands)
                {
                    foreach (string entry in command.GetCommmand())
                    {
                        CommandList.Add(new string[] { "Admin Command", entry });
                    }

                }

                foreach (BaseCommand command in module.adminCommands)
                {
                    foreach (string entry in command.GetCommmand())
                    {
                        CommandList.Add(new string[] { "Admin Command", entry });
                    }
                    
                }

                SteamBot.HTMLFileFromArray(HeaderNames, CommandList, module.GetType().ToString());
            }
           
        }

        private class Reboot : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            
            public Reboot(VBot bot, AdminModule module) : base(bot, "!Reboot")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.SteamBot.Reboot();
                return "Rebooted";
            }

        }

        private class Rejoin : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;

            public Rejoin(VBot bot, AdminModule module) : base(bot, "!Rejoin")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.SteamBot.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.LeaveChat);
                module.SteamBot.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.EnterChat);
                return "Rejoined!";
            }

        }

        private class CheckStatus : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;

            public CheckStatus(VBot bot, AdminModule module) : base(bot, "!CheckData")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                return Msg.Sender.Rank.ToString();
            }

        }

        private class Rename : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;

            public Rename(VBot bot, AdminModule module) : base(bot, "!Rename")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] command = param.Split(new char[] { ' ' }, 2);
                if (command.Length > 0)
                {
                    module.SteamBot.Username = command[1];
                    return "Renamed";
                }
                return "There was an error with that name";
            }

        }
        
        private class RemoveModule : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            VBot botty;

            public RemoveModule(VBot bot, AdminModule module) : base(bot, "!ModuleRemove")
            {
                this.module = module;
                botty = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                botty.Disablemodule(param);
                return "Removing Module...";
            }

        }

        private class AddModule : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            VBot botty;

            public AddModule(VBot bot, AdminModule module) : base(bot, "!ModuleAdd")
            {
                this.module = module;
                botty = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                botty.Enablemodule(param);
                return "Adding Module...";
            }

        }

        private class GetAllModules : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            VBot botty;

            public GetAllModules(VBot bot, AdminModule module) : base(bot, "!ModuleList")
            {
                this.module = module;
                botty = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                string Response = "";
                foreach (BaseModule ModuleEntry in botty.ModuleList)
                {
                    Response += ModuleEntry.GetType().Name.ToString() + " ";
                }
                botty.Disablemodule(param);
                return Response;
            }

        }
    }
}