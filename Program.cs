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
            SteamBotData[] SteamBotLoginData = JsonConvert.DeserializeObject<SteamBotData[]>(File.ReadAllText("settings.json")); //Get the data about each bot alongside their info from the JSON file
            SteamBotData Entry = SteamBotLoginData[0];

            VBot VbotHandler = new VBot();
            SteamInterface SteamPlatformInterface = new SteamInterface(Entry, 0, 103582791429594873);
            DiscordInterface DiscordPlatformInterface = new DiscordInterface("MjIyMjA0MDQ2MjYwMzA1OTIy.CrQ1MA.StYrm9OA2qsJxWv9kcD0_GvwBlU", 50);
            Console.WriteLine("Left the discordPlatnform");


            /*
            VbotHandler.AssignAppInterface(SteamPlatformInterface);
            SteamPlatformInterface.AssignUserHandler(VbotHandler);
            */
            ConsoleUserHandler consolehandler = new ConsoleUserHandler();

            
            AssignConnection(VbotHandler, DiscordPlatformInterface);
            AssignConnection(VbotHandler, SteamPlatformInterface);
            AssignConnection(consolehandler, DiscordPlatformInterface);

            
            SteamPlatformInterface.AssignUserHandler(consolehandler);
            consolehandler.AssignAppInterface(SteamPlatformInterface);

            Bots.Add(SteamPlatformInterface);
            Bots.Add(DiscordPlatformInterface);

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
    