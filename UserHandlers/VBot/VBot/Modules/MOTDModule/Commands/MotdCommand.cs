namespace SteamBotLite
{
    internal partial class MotdModule
    {
        // The abstract command for motd

        abstract public class MotdCommand : BaseCommand
        {
            protected MotdModule motd;

            public MotdCommand(VBot bot, string command, MotdModule motd) : base(bot, "!" + motd.GetName() + command)
            {
                this.motd = motd;
            }
        }
    }
}