using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamBotLite
{
    internal partial class RepliesModule
    {
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