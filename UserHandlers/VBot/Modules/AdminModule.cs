using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamKit2;

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
        }

        public override string getPersistentData()
        {
               return "";
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }


        private class Reboot : BaseCommand
        {
            // Command to query if a server is active
            AdminModule module;
            
            public Reboot(VBot bot, AdminModule module) : base(bot, "!Reboot")
            {
                this.module = module;
            }
            protected override string exec(SteamID sender, string param)
            {
                module.SteamBot.Reboot();
                return "Rebooted";
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
            protected override string exec(SteamID sender, string param)
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
            protected override string exec(SteamID sender, string param)
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
            protected override string exec(SteamID sender, string param)
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
            protected override string exec(SteamID sender, string param)
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