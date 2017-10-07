namespace SteamBotLite
{
    public partial class ServerTrackingModule
    {
        // Special status commands

        private class ActiveServers : BaseCommand
        {
            // Command to query if a server is active
            private ServerTrackingModule module;

            public ActiveServers(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!Active")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string activeServers = "";
                foreach (TrackingServerInfo server in module.TrackedServers.Servers)
                    if (server.playerCount > 1)
                    {
                        if (!activeServers.Equals(string.Empty))
                            activeServers += "\n";

                        activeServers += server.ToString();
                    }
                return activeServers.Equals(string.Empty) ? "No server is currently active" : activeServers;
            }
        }
    }
}