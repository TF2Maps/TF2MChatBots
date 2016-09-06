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
        /*
        static void Main(string[] args)
        {
            
            SteamBotData[] Bots = JsonConvert.DeserializeObject<SteamBotData[]>(File.ReadAllText("settings.json")); //Get the data about each bot alongside their info from the JSON file

            List<SteamConnectionHandler> SteamConnections = new List<SteamConnectionHandler>(); //We make a list that'll contain our connections to steam

            int ID = 0;

            foreach (SteamBotData Entry in Bots) //We create an instance of each Bot and add it to the list
            {
                if (Entry.Userhandler != null) //We check if the UserHandler has been set before adding it
                {
                    SteamConnections.Add(new SteamConnectionHandler(Entry,ID)); //This loads the bot, then adds it to the list
                }
                else
                {
                    Console.WriteLine("Failed to load {0} because of an invalid BotControlClass", Entry.username); //Warn the user the bot isn't loaded
                }
                ID++;
            }

            bool Running = true;

            //This loop iterates through each bot in the list, checking it's callbacks for anything to run 
            while (Running) 
            {

                foreach (SteamConnectionHandler Connection in SteamConnections)
                {
                    Connection.Tick();
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        */
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
            
            bool Running = true;
            while (Running)
            {
                SteamPlatformInterface.tick();
                System.Threading.Thread.Sleep(100);
            }
        }
    }
}
    