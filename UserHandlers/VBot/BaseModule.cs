using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SteamBotLite
{
    abstract class BaseModule
    {
        public List<BaseCommand> commands {get; private set;}
        public List<BaseCommand> adminCommands {get; private set;}
        public List<BaseTask> tasks { get; private set;}
        public string SavedData()
        {
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
            System.IO.File.WriteAllText(this.GetType().Name + ".json", jsonData);
        }

        abstract public string getPersistentData();
        abstract public void loadPersistentData();
    }
}
