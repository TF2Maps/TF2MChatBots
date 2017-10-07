using System.Net;

namespace SteamBotLite
{
    public partial class ServerTrackingModule
    {
        private class ServerAdd : BaseCommand
        {
            private ServerTrackingModule module;

            public ServerAdd(ModuleHandler bot, ServerTrackingModule module) : base(bot, "!serveradd")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' });

                if (parameters.Length > 2)
                {
                    try
                    {
                        IPEndPoint ep = new IPEndPoint(System.Net.IPAddress.Parse(parameters[1]), int.Parse(parameters[2]));
                        TrackingServerInfo Server = new TrackingServerInfo(parameters[1], int.Parse(parameters[2]), parameters[0]);
                        module.TrackedServers.Add(Server);

                        return string.Format("Server {0} has been successfully added at: {1}", Server.tag, Server.serverIP);
                    }
                    catch
                    {
                        return "Your data types were invalid!";
                    }
                }
                else
                {
                    return "Your Server was not added, remember the command is: !serveradd servername serverIP serverPort";
                }
            }
        }
    }
}