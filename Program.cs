using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace SteamBotLite
{
    class Program
    {

        static void Main(string[] args)
        {

            
            //Create userHandlers//
            List<UserHandler> UserHandlers = new List<UserHandler>();

            ConsoleUserHandler consolehandler = new ConsoleUserHandler();
            VBot VbotHandler = new VBot();
            GhostChecker ghostchecker = new GhostChecker();
            

            // Create Interfaces//
            List<ApplicationInterface> Bots = new List<ApplicationInterface>();

            SteamAccountVBot SteamPlatformInterface = new SteamAccountVBot();
            DiscordAccountVBot DiscordPlatformInterfaceRelay = new DiscordAccountVBot();

            Bots.Add(SteamPlatformInterface);
            Bots.Add(DiscordPlatformInterfaceRelay);

            //Link userhandlers and classes that are two way//
            AssignConnection(VbotHandler, DiscordPlatformInterfaceRelay);
            AssignConnection(VbotHandler, SteamPlatformInterface);
            AssignConnection(consolehandler, DiscordPlatformInterfaceRelay);
            AssignConnection(consolehandler, SteamPlatformInterface);
            AssignConnection(ghostchecker, SteamPlatformInterface);

            

            Thread[] BotThreads = new Thread[Bots.Count];
            //Start looping and iterating//
            for (int x = 0; x < Bots.Count; x++)
            {
                BotThreads[x] = new Thread(new ThreadStart(Bots[x].StartTickThreadLoop));
                BotThreads[x].Start();
            }
            
            bool Running = true;
            
            while (Running)
            {
                string Message = Console.ReadLine();
                foreach (ApplicationInterface bot in Bots)
                {
                    bot.BroadCastMessage(null, Message);
                }
            }
        }

        public static void AssignConnection (UserHandler userhandler , ApplicationInterface applicationinterface)
        {
            userhandler.AssignAppInterface(applicationinterface);
            applicationinterface.AssignUserHandler(userhandler);
        }

        public void DoWork(ApplicationInterface Bot)
        {
            bool Running = true;
            while (Running)
            {
                Bot.tick();
            }

        }
    }
}
    