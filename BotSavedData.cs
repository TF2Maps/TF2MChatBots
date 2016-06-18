using System;
using System.Collections.Generic;
using SteamKit2;
using System.IO;

namespace SteamBotLite
{
    public class SteamBotData
    {
        public SteamUser.LogOnDetails LoginData = new SteamUser.LogOnDetails();
        public string SavedPassword;
        
       
        public Type Userhandler
        {
            get; set;
        }
        
        public string LoginKey
        {
            get
            {
                return LoginData.LoginKey;
            }
            set
            {
                LoginData.LoginKey = value;
                LoginData.Password = null;
            }
        }

    public string username
        {
            get
            {
                return LoginData.Username;
            }
            set
            {
                LoginData.Username = value;
                Console.WriteLine("Username {0}", username);
            }
        }
        public bool ShouldRememberPassword
        {
            get
            {
                return LoginData.ShouldRememberPassword;
            }
            set
            {
                LoginData.ShouldRememberPassword = true;
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
                SavedPassword = value;
            }
        }
        public string BotControlClass
        {
            
            get
            {
                return Userhandler.ToString();
            }
            set
            {
                Type T = Type.GetType(value);
                if ((T.GetType() != null ) && (T.BaseType.ToString() == "SteamBotLite.UserHandler"))
                    {
                    Userhandler = Type.GetType(value);
                    }
            }
        }
        

    }
}