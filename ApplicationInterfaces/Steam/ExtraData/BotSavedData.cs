using SteamKit2;

namespace SteamBotLite
{
    public struct SteamBotData
    {
        public SteamBotData(string user, string pass, bool shouldrememberpass)
        {
            LoginData = new SteamUser.LogOnDetails();
            LoginData.Password = pass;
            LoginData.Username = user;

            LoginData.ShouldRememberPassword = shouldrememberpass;

            SavedUsername = user;
            SavedPassword = pass;
        }

        /// <summary>
        /// The LoginData that is sent to steam when we attempt to login
        /// </summary>
        public SteamUser.LogOnDetails LoginData;

        /// <summary>
        /// Incase the LoginKey is invalid, we save it here so we can later set the password again
        /// </summary>
        public string SavedPassword;

        public string SavedUsername;

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
    }
}