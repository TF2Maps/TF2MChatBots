namespace SteamBotLite
{
    internal partial class MotdModule
    {
        private class SetExtendedMotd : MotdCommand
        {
            public SetExtendedMotd(VBot bot, MotdModule motd) : base(bot, "extendedset", motd)
            {
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string[] parameters = param.Split(new char[] { ' ' }, 2);

                if (parameters.Length < 2)
                {
                    return "Incorrect syntax, use: <Number of times to post> <motd>";
                }

                if (motd.message != null)
                    return "There is currently a MOTD, please remove it first";

                int Days;
                try
                {
                    Days = int.Parse(parameters[0]);
                }
                catch
                {
                    return "Incorrect syntax, use: <Number of times to post> <motd>";
                }

                motd.message = parameters[1];
                motd.setter = Msg.Sender;
                motd.postCount = 0;
                motd.postCountLimit = Days;
                motd.savePersistentData();
                return motd.GetName() + " Set to: " + motd.message + " | with a post limit of: " + motd.postCountLimit;
            }
        }
    }
}