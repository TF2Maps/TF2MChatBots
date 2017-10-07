using System;
using System.Linq;

namespace SteamBotLite
{
    public partial class ServerTrackingModule
    {
        private sealed class SpecificServerStatus : BaseCommand
        {
            // Automaticaly generated status command for each server under the config
            private ServerTrackingModule ServerTrackingModule;

            public SpecificServerStatus(ModuleHandler bot, ServerTrackingModule module) : base(bot, "")
            {
                ServerTrackingModule = module;
            }

            public override bool CheckCommandExists(MessageEventArgs Msg, string Message)
            {
                foreach (TrackingServerInfo server in ServerTrackingModule.TrackedServers)
                {
                    if (Message.StartsWith(ServerTrackingModule.NameToserverCommand(server.tag), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }

            public override string[] GetCommmand()
            {
                int AmountOfServers = ServerTrackingModule.TrackedServers.Count();
                string[] TrackingServerList = new string[AmountOfServers];

                for (int x = 0; x < AmountOfServers; x++)
                {
                    TrackingServerList[x] = ServerTrackingModule.NameToserverCommand(ServerTrackingModule.TrackedServers.Servers[x].tag);
                }

                return TrackingServerList;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                foreach (TrackingServerInfo server in ServerTrackingModule.TrackedServers)
                {
                    if (server.tag.Equals(param))
                    {
                        return ServerTrackingModule.ServerQuery(server).ToString();
                    }
                }
                return "An error occured when querying the server!";
            }
        }
    }
}