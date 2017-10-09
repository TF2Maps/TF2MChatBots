using System;
using System.Collections.Generic;
using System.IO;

namespace SteamBotLite
{
    public abstract class BaseModule
    {
        public Dictionary<string, object> config;
        public bool DeletableModule = true;
        protected ModuleHandler userhandler;
        public string ModuleSavedDataFilePath() 
        {
            return Path.Combine(userhandler.GetType().Name, this.GetType().Name + ".json"); 
        }

        public BaseModule(ModuleHandler bot, Dictionary<string, Dictionary<string, object>> Config)
        {
            string ThisObject = this.GetType().Name.ToString();
            Console.WriteLine(ThisObject);
            this.config = Config[ThisObject];
            LoadDependencies(bot);
            bot.AddModuleToCurrentModules(this);
            
        }

        public BaseModule(ModuleHandler bot, Dictionary<string, object> Jsconfig)
        {
            this.config = (Dictionary<string, object>)Jsconfig;
            LoadDependencies(bot);
        }

        public List<BaseCommand> adminCommands { get; private set; }
        public List<BaseCommand> commands { get; private set; }
        public List<BaseTask> tasks { get; private set; }

        abstract public string getPersistentData();

        abstract public void loadPersistentData();

        

        public abstract void OnAllModulesLoaded();

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

        private void LoadDependencies(ModuleHandler bot)
        {
            this.userhandler = bot;
            commands = new List<BaseCommand>();
            adminCommands = new List<BaseCommand>();
            tasks = new List<BaseTask>();
        }
    }
}