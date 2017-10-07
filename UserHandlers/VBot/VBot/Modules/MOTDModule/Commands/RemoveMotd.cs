namespace SteamBotLite
{
    internal partial class MotdModule
    {
        private class RemoveMotd : MotdCommand
        {
            public RemoveMotd(VBot bot, MotdModule motd) : base(bot, "Remove", motd)
            {
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                motd.message = null;
                //motd.setter = null; //TODO FIX
                motd.postCount = 0;
                motd.savePersistentData();
                return "Removed MOTD";
            }
        }
    }
}