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
            SteamBotData[] Bots = JsonConvert.DeserializeObject<SteamBotData[]>(File.ReadAllText("settings.json"));

            List<SteamConnectionHandler> SteamConnections = new List<SteamConnectionHandler>();

            foreach (SteamBotData Entry in Bots)
            {
                SteamConnections.Add(new SteamConnectionHandler(Entry));

            }

            bool Running = true;

            while (Running)
            {

                foreach (SteamConnectionHandler Connection in SteamConnections)
                {
                    Connection.Tick();
                }

                // Console.ReadKey();
            }
        }

    }
}
    