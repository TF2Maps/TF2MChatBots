using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SteamBotLite
{
    internal partial class RepliesModule : BaseModule
    {
        public string SaveDataFile;

        private Dictionary<string, string> Responses;

        public RepliesModule(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            SaveDataFile = ModuleSavedDataFilePath();

            Responses = GetDataDictionary();

            commands.Add(new ReplyFunction(bot, this));
            adminCommands.Add(new ReplyAdd(bot, this));
            adminCommands.Add(new ReplyRemove(bot, this));
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
        }

        private Dictionary<string, string> GetDataDictionary()
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(SaveDataFile));
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }
    }
}