using System.Linq;

namespace SteamBotLite
{
    internal partial class RepliesModule
    {
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
    }
}