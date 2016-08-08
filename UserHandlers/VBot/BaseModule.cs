using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SteamBotLite
{
    abstract class BaseModule
    {
        public List<BaseCommand> commands {get; private set;}
        public List<BaseCommand> adminCommands {get; private set;}
        public List<BaseTask> tasks { get; private set;}
        public string ModuleSavedDataFilePath()
        {
            
            return Path.Combine(userhandler.GetType().Name, this.GetType().Name + ".json");
            return this.GetType().Name + ".json";
        }
        
        protected VBot userhandler;

        private Dictionary<string, object> config;

        public BaseModule(VBot bot, Dictionary<string, object> config)
        {
            this.userhandler = bot;
            this.config = config;
            commands = new List<BaseCommand>();
            adminCommands = new List<BaseCommand>();
            tasks = new List<BaseTask>();
        }

        public void savePersistentData()
        {
            string jsonData = getPersistentData();
            if (Directory.Exists(ModuleSavedDataFilePath()))
            {
                System.IO.File.WriteAllText(ModuleSavedDataFilePath(), jsonData);
            }
            else
            {
                Directory.CreateDirectory(userhandler.GetType().Name);
                System.IO.File.WriteAllText(ModuleSavedDataFilePath(), jsonData);
            };
            
        }

        abstract public string getPersistentData();
        abstract public void loadPersistentData();
    }
}
