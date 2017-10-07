using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamBotLite
{
    internal class RepliesModule : BaseModule
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

        private class Reply : BaseCommand
        {
            // Command to query if a server is active
            private RepliesModule module;

            private string reply;

            public Reply(VBot bot, RepliesModule module, KeyValuePair<string, string> entry) : base(bot, entry.Key)
            {
                this.module = module;
                this.reply = entry.Value;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return (reply);
            }
        }

        private class ReplyAdd : BaseCommand
        {
            private RepliesModule replymodule;

            public ReplyAdd(VBot bot, RepliesModule repliesModule) : base(bot, "!replyadd")
            {
                replymodule = repliesModule;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] command = param.Split(new char[] { ' ' }, 2);

                Dictionary<string, string> values = replymodule.GetDataDictionary();

                if (!values.ContainsKey(command[0]) && (command.Length > 1))
                {
                    replymodule.Responses.Add(command[0], command[1]);
                    values.Add(command[0], command[1]);
                    System.IO.File.WriteAllText(replymodule.SaveDataFile, JsonConvert.SerializeObject(values));
                    return "Reply Added";
                }

                return "Reply not added";
            }
        }

        private class ReplyFunction : BaseCommand
        {
            // Command to query if a server is active
            private RepliesModule module;

            public ReplyFunction(VBot bot, RepliesModule module) : base(bot, null)
            {
                this.module = module;
            }

            public override bool CheckCommandExists(MessageEventArgs Msg, string Message)
            {
                if (module.Responses.ContainsKey(Message))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            public override string[] GetCommmand()
            {
                string[] Array = new string[module.Responses.Count];

                for (int i = 0; i < module.Responses.Count; i++)
                {
                    Array[i] = module.Responses.ElementAt(i).Key;
                }

                return Array;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return (module.Responses[param]);
            }
        }

        private class ReplyRemove : BaseCommand
        {
            private RepliesModule replymodule;

            public ReplyRemove(VBot bot, RepliesModule repliesModule) : base(bot, "!ReplyRemove")
            {
                replymodule = repliesModule;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] command = param.Split(new char[] { ' ' }, 2);

                Dictionary<string, string> values = replymodule.GetDataDictionary();

                if (values.ContainsKey(command[0]))
                {
                    values.Remove(command[0]);
                    System.IO.File.WriteAllText(replymodule.SaveDataFile, JsonConvert.SerializeObject(values));
                    replymodule.Responses.Remove(command[0]);
                    return "Reply Removed";
                }

                return "Reply not added";
            }
        }
    }
}