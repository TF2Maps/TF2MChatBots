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
            UserHandler[] UserHandlers = new UserHandler[9];
            VBot Vbot = new VBot();
            UserHandlers[0] = Vbot;

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
            foreach (UserHandler Bot in UserHandlers)
            {

            }


            Bot SteamBot = new Bot(LoginData, UserHandlers[0]); //Load up an instance of bot's class
            SteamBot.Login(); //Log that bot in
            Console.ReadKey();
        }

    }
}
    