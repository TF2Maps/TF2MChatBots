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
            SteamInterface SteamPlatformInterface = new SteamInterface(Entry, 0);
            
            VbotHandler.AssignAppInterface(SteamPlatformInterface);
            SteamPlatformInterface.AssignUserHandler(VbotHandler);

            ConsoleUserHandler consolehandler = new ConsoleUserHandler();
            SteamPlatformInterface.AssignUserHandler(consolehandler);
            consolehandler.AssignAppInterface(SteamPlatformInterface);

            bool Running = true;
            while (Running)
            {
                SteamPlatformInterface.tick();
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
    