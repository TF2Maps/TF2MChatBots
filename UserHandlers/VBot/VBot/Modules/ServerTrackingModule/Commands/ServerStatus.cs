namespace SteamBotLite
{
    public partial class ServerTrackingModule
    {
        private sealed class ServerStatus : BaseCommand
        {
            // Automaticaly generated status command for each server under the config
            private TrackingServerInfo server;

            private ServerTrackingModule ServerTrackingModule;

            public ServerStatus(ModuleHandler bot, TrackingServerInfo server, ServerTrackingModule module) : base(bot, module.NameToserverCommand(server.tag))
            {
                this.server = server;
                ServerTrackingModule = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                TrackingServerInfo status = ServerTrackingModule.ServerQuery(server);
                if (status != null)
                {
                    server.update(status);
                    return server.ToString();
                }
                else
                    return server.tag + " server did not respond";
            }
        }
    }
}