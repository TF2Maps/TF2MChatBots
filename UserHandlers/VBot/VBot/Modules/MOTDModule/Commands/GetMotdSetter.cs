namespace SteamBotLite
{
    internal partial class MotdModule
    {
        private class GetMotdSetter : MotdCommand
        {
            public GetMotdSetter(VBot bot, MotdModule motd) : base(bot, "Setter", motd)
            {
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return motd.GetName() + " set by " + motd.setter;
            }
        }
    }
}