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

        public RepliesModule(VBot bot, Dictionary<string, object> config) : base(bot, config)
        {
            
            SaveDataFile = this.GetType().Name + ".json";

            Dictionary<string, string> values = GetDataDictionary();
                      
            foreach (KeyValuePair<string, string> Responses in values)
            {
                commands.Add(new Reply(bot, this, Responses));
            }
            adminCommands.Add(new ReplyAdd(bot, this));
            adminCommands.Add(new ReplyRemove(bot, this));
            adminCommands.Add(new Reply(bot, this, new KeyValuePair<string, string>("!PathTest", SavedData())));
        }

        public override string getPersistentData()
        {
               return "";
        }

        public override void loadPersistentData()
        {
            throw new NotImplementedException();
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
            protected override string exec(SteamID sender, string param)
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
            protected override string exec(SteamID sender, string param)
            {
                string[] command = param.Split(new char[] { ' ' }, 2);

                Dictionary<string, string> values = replymodule.GetDataDictionary();

                if (!values.ContainsKey(command[0]) && (command.Length > 1))
                {
                    values.Add(command[0],command[1]);
                    System.IO.File.WriteAllText(replymodule.SaveDataFile, JsonConvert.SerializeObject(values));
                    userhandler.chatCommands.Add(new Reply(userhandler, replymodule, new KeyValuePair<string, string>(command[0], command[1])));               
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
            protected override string exec(SteamID sender, string param)
            {
                string[] command = param.Split(new char[] { ' ' }, 2);

                Dictionary<string, string> values = replymodule.GetDataDictionary();

                if (values.ContainsKey(command[0]) && (command.Length > 1))
                {
                    values.Remove(command[0]);
                    System.IO.File.WriteAllText(replymodule.SaveDataFile, JsonConvert.SerializeObject(values));
                    userhandler.chatCommands.Add(new Reply(userhandler, replymodule, new KeyValuePair<string, string>(command[0], command[1])));
                    return "Reply Removed";
                }

                return "Reply not added";
            }
        }
    }
}