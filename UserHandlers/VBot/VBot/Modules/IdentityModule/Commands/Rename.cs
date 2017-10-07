namespace SteamBotLite
{
    public partial class IdentityModule
    {
        private class Rename : BaseCommand
        {
            // Command to query if a server is active
            private IdentityModule module;

            public Rename(ModuleHandler bot, IdentityModule module) : base(bot, "!Rename")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                if (param.Length > 0)
                {
                    module.userhandler.SetUsernameEventProcess(param);

                    module.savePersistentData();
                    return "Renamed";
                }
                else
                {
                    return "There was no name!";
                }
            }
        }
    }
}