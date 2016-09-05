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

            int ID = 0;

            foreach (SteamBotData Entry in SteamBotLoginData) //We create an instance of each Bot and add it to the list
            {
                if (Entry.Userhandler != null) //We check if the UserHandler has been set before adding it
                {
                    Bots.Add(new SteamInterface(Entry, ID)); //This loads the bot, then adds it to the list                    
                    UserHandlers.Add(new VBot(Bots[Bots.Count -1]));
                }
                else
                {
                    Console.WriteLine("Failed to load {0} because of an invalid BotControlClass", Entry.username); //Warn the user the bot isn't loaded
                }
                ID++;
            }
            bool Running = true;
            while (Running)
            {
                foreach (ApplicationInterface Bot in Bots)
                {
                    Bot.tick();
                }
                System.Threading.Thread.Sleep(100);
            }
        }
        void CreateMultiUserHandler(SteamBotData botdata)
        {
            SteamInterface NewInterface = new SteamInterface(botdata, 1);
            SteamInterface NewInterface2 = new SteamInterface(botdata, 2);

            VBot Bot1 = new VBot(NewInterface);
            VBot Bot2 = new VBot(NewInterface);

        }

    }
}
    