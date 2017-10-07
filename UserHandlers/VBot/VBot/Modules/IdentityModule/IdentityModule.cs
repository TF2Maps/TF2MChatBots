using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamBotLite
{
    public partial class IdentityModule : BaseModule, OnLoginCompletedListiners, MapListChangeListiner
    {
        private ModuleHandler modulehandler;
        private string status;
        private string StatusPrefix = "";
        private UserHandler userhandler;
        private string username;
        private bool UseStatus;

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

        public void MaplistChange(IReadOnlyList<Map> maplist)
        {
            StatusPrefix = "[" + maplist.Count + "] ";
            userhandler.SetUsernameEventProcess(username);
            userhandler.SetStatusmessageEvent(StatusPrefix + status);
        }

        public override void OnAllModulesLoaded()
        {
            userhandler.SetUsernameEventProcess(username);
            userhandler.SetStatusmessageEvent(StatusPrefix + status);
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

        private string getusername()
        {
            return username;
        }
    }
}