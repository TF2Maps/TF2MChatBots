using System;
using System.Collections.Generic;
using SteamKit2;
using System.IO;

namespace SteamBotLite
{
    public class SteamBotData
    {
        SteamUser.LogOnDetails LoginData = new SteamUser.LogOnDetails();

        public string username
        {
            get
            {
                return LoginData.Username;
            }
            set
            {
                Console.WriteLine("Username {0}", username);
                LoginData.Username = value;
            }
        }
        public string password
        {
            get
            {
                return LoginData.Password;
            }
            set
            {
                LoginData.Password = value;
            }
        }
        string BotControlClass { get; set; }
        

    }
}