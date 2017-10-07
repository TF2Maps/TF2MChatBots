namespace SteamBotLite
{
    internal partial class MotdModule
    {
        private class GetMOTD : MotdCommand
        {
            public GetMOTD(VBot bot, MotdModule motd) : base(bot, "get", motd)
            {
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                if (motd.postCount >= motd.postCountLimit)
                {
                    motd.message = null;
                    motd.postCount = 0;
                }

                return motd.message;
            }
        }
    }
}