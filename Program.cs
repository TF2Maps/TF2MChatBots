using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamBotLite
{
    class Program
    {


        static void Main(string[] args)
        {
            //Get the login Details we'll use to login
            string[] LoginDetails = new string[2];
            Console.WriteLine("Username:");
            LoginDetails[0] = Console.ReadLine();
            Console.WriteLine("Password:");
            LoginDetails[1] = Console.ReadLine();

            SteamUser.LogOnDetails LoginData = new SteamUser.LogOnDetails
            {

                Username = LoginDetails[0],
                Password = LoginDetails[1]
            };
            
          //  SteamConnectionHandler FirstBot = new SteamConnectionHandler(new VBot(LoginData));

            SteamConnectionHandler[] SteamConnections = {
                new SteamConnectionHandler(new VBot(LoginData)),
                new SteamConnectionHandler(new VBot(LoginData))
            };
            CallbackManager[] manager;

            foreach (SteamConnectionHandler Connection in SteamConnections)
            {
                Connection.Login();
            }

            foreach (SteamConnectionHandler Connection in SteamConnections)
            {
                if (Connection.isRunning)
                {
                    Connection.manager.RunWaitCallbacks();
                }
            }

            Console.ReadKey();
        }

    }
}
    