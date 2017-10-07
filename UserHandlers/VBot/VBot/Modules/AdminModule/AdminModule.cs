using System.Collections.Generic;

namespace SteamBotLite
{
    public partial class AdminModule : BaseModule, OnLoginCompletedListiners
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
    }
}