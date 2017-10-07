using System;

namespace SteamBotLite
{
    public partial class ServerTrackingModule
    {
        private class ServerRemove : BaseCommand
        {
            private ServerTrackingModule module;

            public ServerRemove(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!serverremove")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                foreach (TrackingServerInfo server in module.TrackedServers.Servers)
                {
                    if (param.Equals(server.tag, StringComparison.OrdinalIgnoreCase))
                    {
                        module.TrackedServers.Remove(server);
                        return "The server has been removed from the list";
                    }
                }
                return "Server was not found, remember the servername does not include ! preceeding or 'server' afterwards (EUserver would be EU)";
            }
        }
    }
}