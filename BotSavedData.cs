using System;
using System.Collections.Generic;
using SteamKit2;
using System.IO;

namespace SteamBotLite
{
    public class SteamBotData
    {
        SteamUser.LogOnDetails LoginData = new SteamUser.LogOnDetails();
        Type HandlerType;
        public UserHandler Userhandler
        {
            get
            {
                return (UserHandler)Activator.CreateInstance(
                    HandlerType, new object[] {});
            }
            set
            { }
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
                    HandlerType = Type.GetType(value);
                    }
            }
        }
        

    }
}