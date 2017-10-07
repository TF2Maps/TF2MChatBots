namespace SteamBotLite
{
    internal partial class MotdModule
    {
        // The commands

        private class EmulateMotd : MotdCommand
        {
            public EmulateMotd(VBot bot, MotdModule motd) : base(bot, "Emulate", motd)
            {
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                motd.MotdPost(this, null);
                return "BroadCasted";
            }
        }
    }
}