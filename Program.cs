using SteamBotLite.ApplicationInterfaces.HTTP_Discord;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SteamBotLite
{
    internal class Program
    {
        public static void AssignConnection(UserHandler userhandler, ApplicationInterface applicationinterface)
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

        private static void Main(string[] args)
        {
            
            Console.WriteLine("RUNNING");

            
            ConsoleUserHandler consolehandler = new ConsoleUserHandler();
            MediaBot MediaHandler = new MediaBot();
            VBot vbot_instance = new VBot();
            GhostChecker ghostchecker = new GhostChecker();
            List<UserHandler> user_handlers = new List<UserHandler>() {
                consolehandler, MediaHandler, vbot_instance, ghostchecker
            };
            
            HttpInterface Test_Bot = new HttpInterface();
            SteamAccountVBot SteamPlatformInterface = new SteamAccountVBot();
            List<ApplicationInterface> services = new List<ApplicationInterface>() {
                SteamPlatformInterface, Test_Bot
            };

            foreach(UserHandler handler in user_handlers)
            {
                foreach(ApplicationInterface service_instance in services)
                {
                    AssignConnection(handler, service_instance);
                }
            }

            /*
            ConsoleInterface DebugInterface = new ConsoleInterface();
            if (Console.ReadLine().Equals("Y"))
            {
                bool RunConsole = true;

                while (RunConsole)
                {
                    MessageEventArgs Msg = new MessageEventArgs(DebugInterface);
                    Msg.Sender = new ChatroomEntity("Console", DebugInterface);
                    Msg.ReceivedMessage = Console.ReadLine();
                    Msg.Sender.Rank = ChatroomEntity.AdminStatus.True;
                    VbotHandler.ProcessPrivateMessage(DebugInterface, Msg);
                    Msg.InterfaceHandlerDestination = DebugInterface;

                    if (Msg.ReceivedMessage.Equals("Exit"))
                    {
                        RunConsole = false;
                    }
                }
            }
            */


            Thread[] BotThreads = new Thread[services.Count];

            //Start looping and iterating//
            for (int x = 0; x < services.Count; x++)
            {
                BotThreads[x] = new Thread(new ThreadStart(services[x].StartTickThreadLoop));
                BotThreads[x].Priority = ThreadPriority.BelowNormal;
                BotThreads[x].Start();
            }

            bool Running = true;

            while (Running)
            {
                System.Threading.Thread.Sleep(30000);
            }
        }
    }
}