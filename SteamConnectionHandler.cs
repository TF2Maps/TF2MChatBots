using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.IO;
using System.Security.Cryptography;

namespace SteamBotLite
{
    public class SteamConnectionHandler
    {
        public string user;

        public string pass;

        public string AuthCode;

        public SteamUser.LogOnDetails LoginData;

        string SentryFileName = "sentry.bin";
        
        public UserHandler UserHandlerClass;

        VBot VBot;
        
        SteamClient steamClient;
        public CallbackManager manager;

        public SteamUser steamUser;
        public SteamFriends SteamFriends ;
      
        public bool loggingin;

        public void Tick()
        {
            manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            /*
            if (Steamclient2.IsConnected)
            {
                manager.RunWaitCallbacks(TimeSpan.FromSeconds(1));
            }
            else
            {
                Reconnect();
            }
            */
        }


        public SteamConnectionHandler(SteamBotData BotData) //Now it'll run when the class is initialised, what could go wrong
        {

            UserHandlerClass = LoadUserHandler(BotData.Userhandler);
            LoginData = BotData.LoginData;


            // create our steamclient instance
            steamClient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);
            
            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            //Get the steamfriends handler, which is used for communicating with users
            SteamFriends = steamClient.GetHandler<SteamFriends>();
            
            // register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo);
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList);

            //  manager.Subscribe<SteamFriends.FriendMsgCallback>(UserHandlerClass.OnMessage);
            manager.Subscribe<SteamFriends.FriendMsgCallback>(UserHandlerClass.OnMessage);
            manager.Subscribe<SteamFriends.ChatMsgCallback>(UserHandlerClass.OnChatRoomMessage);

            // this callback is triggered when the steam servers wish for the client to store the sentry file
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            loggingin = true;

            Console.WriteLine("Connecting User: {0}", user);

            Console.WriteLine("Connecting to Steam...");

            SteamDirectory.Initialize().Wait(); //Gets a new server list, this is REALLY necessary. 

            // initiate the connection
            steamClient.Connect();
            
        }
        UserHandler LoadUserHandler (Type HandlerType)
        {
            return (UserHandler)Activator.CreateInstance(
                    HandlerType, new object[] {this });
        }

        void OnConnected(SteamClient.ConnectedCallback callback )
        {
            if (callback.Result != EResult.OK)
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result);
              //isRunning = false;
                return;
            }

            Console.WriteLine("Connected to Steam! Logging in '{0}'...", user);
            
            //SentryFileName = UserHandlerClass + "_sentry.bin";

            byte[] sentryHash = null;

            if (File.Exists(SentryFileName)) //This allows us to sort sentry files based on userhandlers
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFile = File.ReadAllBytes(SentryFileName);
                sentryHash = CryptoHelper.SHAHash(sentryFile);
                LoginData.SentryFileHash = sentryHash;
            }

            Login(LoginData);

        }

        void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            // at this point, the client has received it's friends list

            int friendCount = SteamFriends.GetFriendCount();

            Console.WriteLine("We have {0} friends", friendCount);
        }

        void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            SteamFriends.SetPersonaState(EPersonaState.Online);
            Console.WriteLine("We have {0} friends", SteamFriends.GetFriendCount());
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
          //  Console.WriteLine(LoginDetails.SentryFileHash.ToString());
            steamUser.LogOn(LoginDetails);
        }

        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from Steam");
            Reconnect();
            //Login();
        }

        void Reconnect()
        {
            // isRunning = false;
            SteamDirectory.Initialize().Wait(); //Update internal list that is heavily used. 
            steamClient.Connect(); //Lets try and log back in
        }

        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied;
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor;

            if (callback.Result != EResult.OK) //If we didn't log in
            {
                if (isSteamGuard || is2FA) //Check what steamguard protection is used 
                {
                    Console.WriteLine("This account is SteamGuard protected!");

                    if (is2FA)
                    {
                        Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                        LoginData.TwoFactorCode = Console.ReadLine();
                    }
                    else
                    {
                        Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                        LoginData.AuthCode = Console.ReadLine();
                    }

                    return;
                }
                else
                {
                    Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult);
                    Console.WriteLine("This error is more indicative of an incorrect username + password");
                    return;
                }
            }
            else
            {
                Console.WriteLine("Successfully logged on!");
                Console.WriteLine(steamClient.IsConnected);
                Console.WriteLine(SteamFriends.GetFriendCount().ToString());
            }
        }
        void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open(SentryFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                fs.Seek(callback.Offset, SeekOrigin.Begin);
                fs.Write(callback.Data, 0, callback.BytesToWrite);
                fileSize = (int)fs.Length;

                fs.Seek(0, SeekOrigin.Begin);
                using (var sha = new SHA1CryptoServiceProvider())
                {
                    sentryHash = sha.ComputeHash(fs);
                }
            }

            // inform the steam servers that we're accepting this sentry file
            steamUser.SendMachineAuthResponse(new SteamUser.MachineAuthDetails
            {
                JobID = callback.JobID,

                FileName = callback.FileName,

                BytesWritten = callback.BytesToWrite,
                FileSize = fileSize,
                Offset = callback.Offset,

                Result = EResult.OK,
                LastError = 0,

                OneTimePassword = callback.OneTimePassword,

                SentryFileHash = sentryHash,
            });

            Console.WriteLine("Done!");
        }


        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }
    }
}
