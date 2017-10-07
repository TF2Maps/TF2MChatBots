namespace SteamBotLite
{
    public partial class AdminModule
    {
        private class GetAllModules : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            private ModuleHandler modulehandler;

            public GetAllModules(ModuleHandler bot, AdminModule module) : base(bot, "!ModuleList")
            {
                this.module = module;
                this.modulehandler = bot;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string Response = "";

                foreach (BaseModule ModuleEntry in modulehandler.GetAllModules())
                {
                    Response += ModuleEntry.GetType().Name.ToString() + " ";
                }

                return Response;
            }
        }
    }
}