namespace SteamBotLite
{
    internal partial class MotdModule
    {
        private class CurrentMotdTick : MotdCommand
        {
            public CurrentMotdTick(VBot bot, MotdModule motd) : base(bot, "Tick", motd)
            {
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return motd.GetName() + " displayed " + motd.postCount + " times and will display a total of " + motd.postCountLimit + " times in total";
            }
        }
    }
}