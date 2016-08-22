using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

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

        public bool DeletableModule = true;
        public bool AutoLoadModule = true;

        private Dictionary<string, object> ConfigFilePath = JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(@"config.json"));
        public Dictionary<string, object> config;

        public BaseModule(VBot bot, Dictionary<string, object> Jsconfig)
        {
            this.config = JsonConvert.DeserializeObject<Dictionary<string, object>>(Jsconfig[this.GetType().Name].ToString());
            this.userhandler = bot;            
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
