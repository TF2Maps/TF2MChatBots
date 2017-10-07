namespace SteamBotLite
{
    public partial class ServerTrackingModule
    {
        // Other commands
        private class FullServerQuery : BaseCommand
        {
            // Command to query if a server is active
            private ServerTrackingModule module;

            public FullServerQuery(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!Serverquery")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string activeServers = "";
                foreach (TrackingServerInfo server in module.TrackedServers.Servers)
                {
                    if (!activeServers.Equals(string.Empty))
                    {
                        activeServers += "\n";
                    }
                    activeServers += server.ToString();
                }
                return activeServers.Equals(string.Empty) ? "No server is currently active" : activeServers;
            }
        }
    }
}