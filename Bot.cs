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
        SteamFriends steamFriends;

        bool isRunning;

        public void Login()
        {
            // create our steamclient instance
            steamClient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            steamFriends = steamClient.GetHandler<SteamFriends>();

           

            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);


            manager.Subscribe<SteamFriends.FriendMsgCallback>(OnPersonalMessage);
            manager.Subscribe<SteamFriends.ChatMsgCallback>(OnChatRoomMessage);


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
        void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            // at this point, the client has received it's friends list

            int friendCount = steamFriends.GetFriendCount();

            Console.WriteLine("We have {0} friends", friendCount);
        }

            void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            steamFriends.SetPersonaState(EPersonaState.Online);
        }

        void OnPersonalMessage(SteamFriends.FriendMsgCallback msg)
        {
            if (msg.Message != "")
            {
                Console.WriteLine("Personal Message from {0}: {1}", msg.Sender, msg.Message);
            }
        }
        void OnChatRoomMessage(SteamFriends.ChatMsgCallback msg)
        {
            if (msg.Message != "")
            {
                Console.WriteLine("Chatroom Message from {0}: {1}", msg.ChatterID, msg.Message);
            }
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
                Console.WriteLine(steamClient.IsConnected);
                Console.WriteLine(steamFriends.GetFriendCount().ToString());
            }

        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }
    }
}
