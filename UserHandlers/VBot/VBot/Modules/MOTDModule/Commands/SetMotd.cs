using System;

namespace SteamBotLite
{
    internal partial class MotdModule
    {
        private class SetMotd : MotdCommand
        {
            public SetMotd(VBot bot, MotdModule motd) : base(bot, "Set", motd)
            {
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                if (param == String.Empty)
                    return "Make sure to include a MOTD to display!";

                if (motd.message != null)
                    return "There is currently a MOTD, please remove it first";

                motd.message = param;
                motd.setter = Msg.Sender;
                motd.postCount = 0;
                motd.postCountLimit = motd.DefaultPostCountLimit();
                motd.savePersistentData();
                return motd.GetName() + " Set to: " + motd.message;
            }
        }
    }
}