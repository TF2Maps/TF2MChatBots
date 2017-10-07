using System.Diagnostics;

namespace SteamBotLite
{
    public partial class AdminModule
    {
        private class RunScript : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public RunScript(ModuleHandler bot, AdminModule module) : base(bot, "!RunUpdateScript")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                Process proc = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "../../update.sh"
                    }
                };

                proc.Start();

                Process.GetCurrentProcess().Kill();
                return "Script should've ran";
            }
        }
    }
}