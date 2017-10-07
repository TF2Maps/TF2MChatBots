namespace SteamBotLite
{
    public partial class IdentityModule
    {
        private class CheckStatus : BaseCommand
        {
            // Command to query if a server is active
            private IdentityModule module;

            public CheckStatus(ModuleHandler bot, IdentityModule module) : base(bot, "!CheckData")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return Msg.Sender.Rank.ToString();
            }
        }
    }
}