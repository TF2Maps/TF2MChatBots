using System;

namespace SteamBotLite
{
    public partial class IdentityModule
    {
        private class UnsetStatusMessage : BaseCommand
        {
            // Command to query if a server is active
            private UserHandler bot;

            private IdentityModule module;

            public UnsetStatusMessage(UserHandler bot, ModuleHandler modulehandler, IdentityModule module) : base(modulehandler, "!StatusRemove")
            {
                this.module = module;
                this.bot = bot;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.UseStatus = false;
                module.savePersistentData();

                bot.SetStatusmessageEvent(null);
                return "Status has been removed";
            }

            private void Bot_SetStatusmessage(object sender, string e)
            {
                throw new NotImplementedException();
            }
        }
    }
}