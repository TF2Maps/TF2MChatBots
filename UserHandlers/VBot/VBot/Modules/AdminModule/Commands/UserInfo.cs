namespace SteamBotLite
{
    public partial class AdminModule
    {
        private class UserInfo : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public UserInfo(ModuleHandler bot, AdminModule module) : base(bot, "!UserInfo")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return string.Format("Your ID is:: {0} | {1} | {2}", Msg.Sender.identifier, Msg.Sender.Rank, Msg.Sender.DisplayName);
            }
        }
    }
}