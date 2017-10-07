using System;

namespace SteamBotLite
{
    public partial class IdentityModule
    {
        private class SetStatusMessage : BaseCommand
        {
            // Command to query if a server is active
            private UserHandler bot;

            private IdentityModule module;

            public SetStatusMessage(UserHandler bot, ModuleHandler modulehandler, IdentityModule module) : base(modulehandler, "!StatusSet")
            {
                this.module = module;
                this.bot = bot;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                module.status = param;
                module.UseStatus = true;
                module.savePersistentData();
                bot.SetStatusmessageEvent(param);

                return "Status has been updated";
            }

            private void Bot_SetStatusmessage(object sender, string e)
            {
                throw new NotImplementedException();
            }
        }
    }
}