using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SteamKit2;

namespace SteamBotLite
{
    class RepliesModule : BaseModule
    {
       
        public string SaveDataFile;

        Dictionary<string, string> Responses;

        public RepliesModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            
            SaveDataFile = this.GetType().Name + ".json";

            Responses = GetDataDictionary();

            commands.Add(new ReplyFunction(bot, this));
            adminCommands.Add(new ReplyAdd(bot, this));
            adminCommands.Add(new ReplyRemove(bot, this));            
        }

        public override void OnAllModulesLoaded()
        {


        }

        public override string getPersistentData()
        {
               return "";
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
        }

        private class ReplyFunction : BaseCommand
        {
            // Command to query if a server is active
            RepliesModule module;

            public ReplyFunction(VBot bot, RepliesModule module) : base(bot, null)
            {
                this.module = module;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                return (module.Responses[param]);
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
        }


        private class Reply : BaseCommand
        {
            // Command to query if a server is active
            RepliesModule module;
            string reply;
            public Reply(VBot bot, RepliesModule module, KeyValuePair<string,string> entry) : base(bot, entry.Key)
            {
                this.module = module;
                this.reply = entry.Value;
            }
            protected override string exec(MessageEventArgs Msg, string param)
            {
                return (reply);

            }

        }
        Dictionary<string, string> GetDataDictionary ()
        {
            try
            {
                return JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(SaveDataFile));
            }
            catch
            {
                
                return new Dictionary<string,string>();
            }
        }
        
        private class ReplyAdd : BaseCommand
        {
            RepliesModule replymodule;
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
                    values.Add(command[0],command[1]);
                    System.IO.File.WriteAllText(replymodule.SaveDataFile, JsonConvert.SerializeObject(values));
                    return "Reply Added";
                }

                return "Reply not added";
            }
        }

        private class ReplyRemove : BaseCommand
        {
            RepliesModule replymodule;
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
                    BaseCommand ReplyToRemove = replymodule.commands.FirstOrDefault(x => x.command == command[0]);
                    return "Reply Removed";
                }

                return "Reply not added";
            }
        }
    }
}