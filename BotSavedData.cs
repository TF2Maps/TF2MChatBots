using System;
using System.Collections.Generic;
using SteamKit2;
using System.IO;

namespace SteamBotLite
{
    public class SteamBotData
    {
        /// <summary>
        /// The LoginData that is sent to steam when we attempt to login
        /// </summary>
        public SteamUser.LogOnDetails LoginData = new SteamUser.LogOnDetails();

        /// <summary>
        /// Incase the LoginKey is invalid, we save it here so we can later set the password again
        /// </summary>
        public string SavedPassword;

        /// <summary>
        /// The userhandler this bot utilises
        /// </summary>
        public Type Userhandler;

        /// <summary>
        /// The login key allows us to login without SteamAuth
        /// Upon receiving the login Key, we set the password that is sent to steam as null as sending both wont work
        /// </summary>
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
        /// <summary>
        /// The username used to log onto steam with
        /// </summary>
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
        /// <summary>
        /// Setting this to true will allow the bot to auto-login by generating a login-key 
        /// </summary>
        public bool ShouldRememberPassword
        {
            get
            {
                return LoginData.ShouldRememberPassword;
            }
            set
            {
                LoginData.ShouldRememberPassword = value;
            }
        }
        /// <summary>
        /// The password that is sent to Steam. It is set both in the LoginData object and SavedPassword fields, 
        /// however upon receiving a login key it is deleted from the Login Data object to allow the loginkey's usage
        /// </summary>
        public string password
        {
            get
            {
                return LoginData.Password; //We get the Password that is sent to steam
            }
            set
            {
                LoginData.Password = value;  //We set the password used to login with
                SavedPassword = value; //We keep a back up of the password in case the login key we receive later fails
            }
        }
        /// <summary>
        /// The class of the Bot we want to run. There are checks to verify that it inherits the "UserHandler" class, as well as if it exists. 
        ///
        /// </summary>
        public string BotControlClass
        {
            
            get
            {
                return Userhandler.ToString(); //We return the Userhandler assigned
            }
            set
            {
                Type T = Type.GetType(value); //We attempt to translate the string to an existing type
                if ((T.GetType() != null ) && (T.BaseType.ToString() == "SteamBotLite.UserHandler")) //Then we check its valid AND if its a base of userhandler
                    {
                    Userhandler = Type.GetType(value); //If we pass the checks, we set it to the Userhandler
                    }
                
            }
        }
        

    }
}