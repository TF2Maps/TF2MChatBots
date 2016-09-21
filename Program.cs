using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.IO;
using Newtonsoft.Json;

namespace SteamBotLite
{
    class Program
    {
        static void Main(string[] args)
        {
            List<ApplicationInterface> Bots = new List<ApplicationInterface>();
            List<UserHandler> UserHandlers = new List<UserHandler>();
           
            VBot VbotHandler = new VBot();

            SteamInterface SteamPlatformInterface = new SteamAccountVBot();
            DiscordInterface DiscordPlatformInterfaceFun = new DiscordAccountFun();
            DiscordInterface DiscordPlatformInterfaceRelay = new DiscordAccountVBot();
            Console.WriteLine("Left the discordPlatnform");

            ConsoleUserHandler consolehandler = new ConsoleUserHandler();

            AssignConnection(VbotHandler, DiscordPlatformInterfaceRelay);
            AssignConnection(VbotHandler, DiscordPlatformInterfaceFun);
            AssignConnection(VbotHandler, SteamPlatformInterface);

            AssignConnection(consolehandler, DiscordPlatformInterfaceRelay);

            SteamPlatformInterface.AssignUserHandler(consolehandler);
            consolehandler.AssignAppInterface(SteamPlatformInterface);

            Bots.Add(SteamPlatformInterface);
            Bots.Add(DiscordPlatformInterfaceRelay);
            Bots.Add(DiscordPlatformInterfaceFun);

            bool Running = true;
            while (Running)
            {
                foreach (ApplicationInterface bot in Bots)
                {
                    bot.tick();
                }
                System.Threading.Thread.Sleep(100);
            }
        }

        public static void AssignConnection (UserHandler userhandler , ApplicationInterface applicationinterface)
        {
            userhandler.AssignAppInterface(applicationinterface);
            applicationinterface.AssignUserHandler(userhandler);
        }
    }
}
    