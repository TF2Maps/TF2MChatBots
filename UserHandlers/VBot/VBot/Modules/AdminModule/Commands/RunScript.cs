using System;
using System.Diagnostics;
using System.IO;

namespace SteamBotLite
{
    public partial class AdminModule
    {
        private class RunScript : BaseCommand
        {
            // Command to query if a server is active
            private AdminModule module;

            public RunScript(ModuleHandler bot, AdminModule module) : base(bot, "!runscript")
            {
                this.module = module;
            }

            protected override string exec(MessageEventArgs Msg, string param)
            {
                string filepath = System.AppDomain.CurrentDomain.BaseDirectory + "/scripts";
                if (Directory.Exists(filepath) == false)
                {
                    try
                    {
                        Directory.CreateDirectory(filepath);
                        return "The directory didn't exist, so it has been created";
                    }
                    catch (Exception e)
                    {
                        return "The directory doesn't exist, and trying to make it caused an error!";
                    }
                }

                if (param.ToLower().Equals("!runscript"))
                {
                    Console.WriteLine("Param is empty?");
                    DirectoryInfo d = new DirectoryInfo(filepath);

                    String output = "";
                    foreach (var file in d.GetFiles("*.sh"))
                    {
                        output += file.Name + ", ";
                    }
                    output.Substring(0, output.Length - 2);
                    return "Scripts allowed: " + output;
                }
                else
                {
                    Console.WriteLine("Not empty");
                    Process proc = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = filepath + '/' + param
                        }
                    };
                    proc.Start();
                    
                    return "Executed script: " + param;
                }
            }
        }
    }
}