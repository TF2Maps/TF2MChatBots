namespace SteamBotLite
{
    internal partial class WebServerHostingModule
    {
        private class RemoveTable : BaseCommand
        {
            private string address;

            // Command to query if a server is active
            private WebServerHostingModule module;

            private ModuleHandler ModuleHandler;

            public RemoveTable(ModuleHandler bot, WebServerHostingModule module) : base(bot, "!RemoveTable")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.RemoveTableFromEntry(param);
                return "Removing table";
            }
        }
    }
}