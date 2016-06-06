using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamBotLite
{
    public class Bot
    {
        public string user;

        public string pass;

        public string AuthCode;

        public SteamUser.LogOnDetails LoginData;


        public Bot(SteamUser.LogOnDetails LoginDataReceived)
        {
            this.LoginData = LoginDataReceived;
        }

        SteamClient steamClient;
        CallbackManager manager;

        SteamUser steamUser;

        bool isRunning;

        public void Login()
        {
            // create our steamclient instance
            steamClient = new SteamClient();
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnMessage);

            isRunning = true;
            Console.WriteLine("User: {0} Pass {1}", user, pass);

            Console.WriteLine("Connecting to Steam...");

            SteamDirectory.Initialize().Wait();

            // initiate the connection
            steamClient.Connect();
            

            // create our callback handling loop
            while (isRunning)
            {
                // in order for the callbacks to get routed, they need to be handled by the manager
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
        }

        void OnConnected(SteamClient.ConnectedCallback callback )
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);

               // isRunning = false;
                return;
            }

            Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);

            
            Login(LoginData);


        }



        void OnMessage(SteamFriends.FriendMsgCallback MessageText)
        {
            Console.WriteLine(MessageText);
        }



        void Login (SteamUser.LogOnDetails LoginDetails)
        {
            steamUser.LogOn(LoginDetails);
        }

        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");
         //   isRunning = false;
            SteamDirectory.Initialize().Wait(); //Update internal list that is heavily used. 
            steamClient.Connect();
            //Login();
        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            if (callback.Result != EResult.OK)
            {
                if (callback.Result == EResult.AccountLogonDenied)
                {
                    // if we recieve AccountLogonDenied or one of it's flavors (AccountLogonDeniedNoMailSent, etc)
                    // then the account we're logging into is SteamGuard protected
                    // see sample 5 for how SteamGuard can be handled

                    Console.WriteLine("Unable to logon to Steam: This account is SteamGuard protected.");
                    Console.WriteLine("Please add SteamAuth code:");
                    LoginData.AuthCode = Console.ReadLine();
                    Login(LoginData);
                    //isRunning = false;
                    

                    return;
                }
                
                Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);

               // isRunning = false;
                return;
            }

            Console.WriteLine("Successfully logged on!");
            Console.ReadKey();
            // at this point, we'd be able to perform actions on Steam

            // for this sample we'll just log off
          //  steamUser.LogOff();
        }

        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }
    }
}
