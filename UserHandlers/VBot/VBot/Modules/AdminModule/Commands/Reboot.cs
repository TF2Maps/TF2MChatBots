namespace SteamBotLite
{
    public partial class AdminModule
    {
        private class Reboot : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public Reboot(ModuleHandler bot, AdminModule module) : base(bot, "!Reboot")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
              //  module.userhandler.Reboot();
                return "This feature has been depreciated";
            }
        }
    }
}