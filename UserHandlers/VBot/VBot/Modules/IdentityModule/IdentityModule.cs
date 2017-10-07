using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;


namespace SteamBotLite
{
    public class IdentityModule : BaseModule, OnLoginCompletedListiners, MapListChangeListiner
    {
        UserHandler userhandler;
        ModuleHandler modulehandler;
        string StatusPrefix = "";
        string username;
        string status;
        bool UseStatus;

        public IdentityModule(UserHandler bot, ModuleHandler handler, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(handler, Jsconfig)
        {
            handler.AddListChangeEventListiner(this);

            DeletableModule = false;
            userhandler = bot;
            this.modulehandler = handler;

            username = config["DefaultUsername"].ToString();
            status = config["DefaultStatus"].ToString();
            UseStatus = bool.Parse(config["UseStatus"].ToString());
            loadPersistentData();
            savePersistentData();

            handler.AddLoginEventListiner(this);


            adminCommands.Add(new Rename(handler, this));
            adminCommands.Add(new SetStatusMessage(userhandler, modulehandler, this));
            adminCommands.Add(new UnsetStatusMessage(userhandler, modulehandler, this));
        }

        public override void OnAllModulesLoaded()
        {
            userhandler.SetUsernameEventProcess(username);
            userhandler.SetStatusmessageEvent(StatusPrefix + status);
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

        string getusername()
        {
            return username;
        }
        public void OnLoginCompleted()
        {
            if (UseStatus)
            {
                userhandler.SetStatusmessageEvent(StatusPrefix + status);
            }
            modulehandler.UpdateUsernameEvent(this, getusername());
            //userhandler.SetStatusmessageEvent(StatusPrefix + status);
        }

        public void MaplistChange(IReadOnlyList<Map> maplist)
        {
            StatusPrefix = "[" + maplist.Count + "] ";
            userhandler.SetUsernameEventProcess(username);
            userhandler.SetStatusmessageEvent(StatusPrefix + status);

        }

        private class SetStatusMessage : BaseCommand
        {
            // Command to query if a server is active
            UserHandler bot;
            IdentityModule module;

            public SetStatusMessage(UserHandler bot, ModuleHandler modulehandler, IdentityModule module) : base(modulehandler, "!StatusSet")
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
            UserHandler bot;
            IdentityModule module;
            public UnsetStatusMessage(UserHandler bot, ModuleHandler modulehandler, IdentityModule module) : base(modulehandler, "!StatusRemove")
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


        private class CheckStatus : BaseCommand
        {
            // Command to query if a server is active
            IdentityModule module;

            public CheckStatus(ModuleHandler bot, IdentityModule module) : base(bot, "!CheckData")
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
            IdentityModule module;

            public Rename(ModuleHandler bot, IdentityModule module) : base(bot, "!Rename")
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                if (param.Length > 0)
                {
                    module.userhandler.SetUsernameEventProcess(param);

                    module.savePersistentData();
                    return "Renamed";
                }
                else
                {
                    return "There was no name!";
                }
            }

        }


    }
}