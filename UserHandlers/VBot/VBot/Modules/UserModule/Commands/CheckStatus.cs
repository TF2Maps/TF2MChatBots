namespace SteamBotLite
{
    public partial class UsersModule
    {
        private class CheckStatus : BaseCommand
        {
            private UsersModule module;

            public CheckStatus(ModuleHandler bot, UsersModule module) : base(bot, "!CheckAdmin")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                return module.admincheck(Msg.Sender).ToString();
            }
        }
    }
}