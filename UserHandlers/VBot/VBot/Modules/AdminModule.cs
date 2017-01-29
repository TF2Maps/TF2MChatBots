using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace SteamBotLite
{
    class AdminModule : BaseModule , OnLoginCompletedListiners
    {
       
        VBot vbot;
        string username;
        string status;
        bool UseStatus;

        public AdminModule(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            DeletableModule = false;
            vbot = bot;
            
            username = config["DefaultUsername"].ToString();
            status = config["DefaultStatus"].ToString();
            UseStatus = bool.Parse(config["UseStatus"].ToString());
            loadPersistentData();
            savePersistentData();

            bot.OnLoginlistiners.Add(this);
            
            adminCommands.Add(new Reboot(bot, this));
            adminCommands.Add(new Rename(bot, this));
            adminCommands.Add(new RemoveModule(bot, this));
            adminCommands.Add(new AddModule(bot, this));
            adminCommands.Add(new GetAllModules(bot, this));
            adminCommands.Add(new Rejoin(bot, this));
            adminCommands.Add(new SetStatusMessage(bot, this));
            adminCommands.Add(new UnsetStatusMessage(bot, this));
        }

        public override string getPersistentData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("Username", username);
            data.Add("Status", status);
            data.Add("UserStatus", UseStatus.ToString());
            return JsonConvert.SerializeObject(data);
        }

        public override void loadPersistentData()
        {
            try
            {
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(this.GetType().Name + ".json"));
                username = data["Username"];
                status = data["Status"];
            }
            catch
            {

            }
        }

        public override void OnAllModulesLoaded()
        {
            foreach(BaseModule module in vbot.ModuleList)
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

                vbot.HTMLFileFromArray(HeaderNames, CommandList, module.GetType().ToString());
            }
           
        }

        public void OnLoginCompleted()
        {
            if (UseStatus)
            {
                vbot.SetStatusmessageEvent(status);
            }
            vbot.Username = username;
        }

        private class SetStatusMessage : BaseCommand
        {
            // Command to query if a server is active
            VBot bot;
            AdminModule module;

            public SetStatusMessage(VBot bot, AdminModule module) : base(bot, "!StatusSet")
            {
                this.module = module;
                this.bot = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.status = param;
                module.UseStatus = true;
                module.savePersistentData();
                bot.SetStatusmessageEvent(param);
                
                return "Status has been updated";
            }

            private void Bot_SetStatusmessage(object sender, string e)
            {
                throw new NotImplementedException();
            }
        }

        private class UnsetStatusMessage : BaseCommand
        {
            // Command to query if a server is active
            VBot bot;
            AdminModule module;
            public UnsetStatusMessage(VBot bot, AdminModule module) : base(bot, "!StatusRemove")
            {
                this.module = module;
                this.bot = bot;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.UseStatus = false;
                module.savePersistentData();
                
                bot.SetStatusmessageEvent(null);
                return "Status has been removed";
            }

            private void Bot_SetStatusmessage(object sender, string e)
            {
                throw new NotImplementedException();
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
                module.vbot.Reboot();
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

                module.vbot.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.LeaveChat);
                module.vbot.FireMainChatRoomEvent(UserHandler.ChatroomEventEnum.EnterChat);
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

                if (param.Length > 0)
                {
                    module.vbot.Username = param;
                    module.username = param;
                    module.savePersistentData();
                    return "Renamed";
                }
                else
                {
                    return "There was no name!";
                }
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