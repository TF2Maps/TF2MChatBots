using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamBotLite
{
    internal partial class RepliesModule
    {
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
    }
}