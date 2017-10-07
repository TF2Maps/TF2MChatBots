namespace SteamBotLite
{
    internal partial class WebServerHostingModule
    {
        private class RebootModule : BaseCommand
        {
            private string address;

            // Command to query if a server is active
            private WebServerHostingModule module;

            private ModuleHandler ModuleHandler;

            public RebootModule(ModuleHandler bot, WebServerHostingModule module) : base(bot, "!WebsiteReboot")
            {
                this.module = module;
                this.address = (module.config["Address"].ToString());
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.CloseWebServer();
                module.StartWebServer(address);
                return "Rebooting Serer";
            }
        }
    }
}