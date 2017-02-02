using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;
using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using SteamKit2.Internal;

namespace SteamBotLite
{
    public abstract class SteamInterface : ApplicationInterface
    {
        
        /// <summary>
        /// We store this, in case we need to reboot
        /// </summary>
        public SteamBotData SteamBotLiteLoginData;
        /// <summary>
        /// Bot's ID
        /// </summary>
        public int ID;
        /// <summary>
        /// Username we will login with
        /// </summary>
        public string user;

        /// <summary>
        /// Password we will login With
        /// </summary>
        private string pass;

        /// <summary>
        /// Login Data that is sent to steam when we attempt logging in
        /// </summary>
        public SteamUser.LogOnDetails LoginData;
        /// <summary>
        /// File name of the sentryfile
        /// </summary>
        private string SentryFileName = "sentry.bin";
        /// <summary>
        /// File name of the loginkey
        /// </summary>
        private string LoginKeyName = "key.txt";
        /// <summary>
        /// The path to the sentry file
        /// </summary>
        string SentryFile;
        /// <summary>
        /// The path to the login file
        /// </summary>
        string LoginKeyFile;

        
        SteamClient steamClient;
        public SteamUser steamUser;
        public SteamFriends SteamFriends;

        /// <summary>
        /// Manages and routes each callback
        /// </summary>
        public CallbackManager manager;

        /// <summary>
        /// This method will check for any Callbacks to fire
        /// </summary>
        public override void tick()
        {
            try
            {
                manager.RunCallbacks();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Handled: {0}", ex);
            }
        }
        public SteamInterface()
        {
            string user = config["username"].ToString();
            string pass = config["password"].ToString();
            bool shouldrememberpass = (bool)config["ShouldRememberPassword"];
            SteamBotData SteamBotLoginData = new SteamBotData(user,pass, shouldrememberpass);
            
            LoginData = SteamBotLoginData.LoginData;
            ResetConnection(SteamBotLoginData);
        }
        

        

        /// <summary>
        /// Creates an instance of SteamConnectionHandler with the data given and logs in, also can be fired to reset the bot
        /// </summary>
        /// <param name="BotData"> Data involving the userhandler and what bot to load</param>
        public void ResetConnection(SteamBotData BotData)
        {
            Console.WriteLine("Loading New Connection");
            SteamBotLiteLoginData = BotData;
            pass = BotData.SavedPassword; //We'll save this now, so we can access it later for logging in if the loginkey fails

            LoginData = BotData.LoginData; //Lets save the login data

            if (!Directory.Exists(user)) //Check if we have a settings folder for this username
            {
                Directory.CreateDirectory(LoginData.Username); //We will create the directory which will be used to store the loginkey and sentry file
            }

            SentryFile = Path.Combine(LoginData.Username, SentryFileName); //Now that we have verified the folder exists, we will set file's path

            LoginKeyFile = Path.Combine(LoginData.Username, LoginKeyName); //Now that we have verified the folder exists, we will set file's path

            if (File.Exists(LoginKeyFile) && LoginData.ShouldRememberPassword == true) //Lets see if a previously set login Key exists
            {
                LoginData.LoginKey = File.ReadAllText(LoginKeyFile); //Lets get the LoginKey and set it
                LoginData.Password = null;
            }

           

            // create our steamclient instance
            steamClient = new SteamClient(System.Net.Sockets.ProtocolType.Tcp);

            // create the callback manager which will route callbacks to function calls
            manager = new CallbackManager(steamClient);

            // get the steamuser handler, which is used for logging on after successfully connecting
            steamUser = steamClient.GetHandler<SteamUser>();

            //Get the steamfriends handler, which is used for communicating with users
            SteamFriends = steamClient.GetHandler<SteamFriends>();

            // Register a few callbacks we're interested in
            // these are registered upon creation to a callback manager, which will then route the callbacks
            // to the functions specified

            //These callbacks are to handle thh connection
            manager.Subscribe<SteamClient.ConnectedCallback>(OnConnected);
            manager.Subscribe<SteamClient.DisconnectedCallback>(OnDisconnected);

            manager.Subscribe<SteamUser.LoggedOnCallback>(OnLoggedOn);
            manager.Subscribe<SteamUser.LoggedOffCallback>(OnLoggedOff);

            //These callbacks are to handler the bot being online/offline 

            //In the future, these callbacks should be migrated over to the UserHandler to utilise instead
            manager.Subscribe<SteamUser.AccountInfoCallback>(OnAccountInfo); //We receive this when we're logged-in, the method referenced makes us go online on community
            manager.Subscribe<SteamFriends.FriendsListCallback>(OnFriendsList); //We receive this when we go online on community


            //We will pass some down to the UserHandler instead, as these manage users not the connection
            manager.Subscribe<SteamFriends.FriendMsgCallback>(ReceivePrivateMessage);
            manager.Subscribe<SteamFriends.ChatMsgCallback>(ReceiveChatMessage);
            manager.Subscribe<SteamFriends.ChatMemberInfoCallback>(Chatmemberinfo);

            // This callback is triggered when the steam servers wish for the client to store the sentry file
            manager.Subscribe<SteamUser.UpdateMachineAuthCallback>(OnMachineAuth);

            //We check if the user wants to remember the password, if so we set the callback up to receive login Keys
            if (BotData.ShouldRememberPassword)
            {
                manager.Subscribe<SteamUser.LoginKeyCallback>(OnLoginKey);
            }

            Console.WriteLine("Connecting User: {0}", user);

            Console.WriteLine("Connecting to Steam...");

            SteamDirectory.Initialize().Wait(); //Gets a new server list, this is REALLY necessary. 

            // initiate the connection
            Reconnect();

        }

       
        /// <summary>
        /// Callback fired when get get chat member info 
        /// </summary>
        /// <param name="Callback"></param>
        void chatmemberinfo(SteamFriends.ChatActionResultCallback Callback)
        {
            Console.WriteLine("Info: " + Callback.ChatterID + ": " + Callback.Action + ", " + Callback.Result);
        }

        /// <summary>
        /// Callback for receiving data about connecting to steam
        /// </summary>
        /// <param name="callback"></param>
        void OnConnected(SteamClient.ConnectedCallback callback)
        {
            if (callback.Result != EResult.OK) //If we're not Logged in
            {
                Console.WriteLine("Unable to connect to Steam: {0}", callback.Result); //We will print out the error
                                                                                       //isRunning = false;
                return; //And return
            }

            Console.WriteLine("Connected to Steam! Logging in '{0}'...", user); //If we did connect though, lets tell the user

            byte[] sentryHash = null; //lets get the sentry Hash which allows multiple steamconnectionhandlers can share a login

            if (File.Exists(SentryFile)) //We'll grab the sentry file if it exists already
            {
                // if we have a saved sentry file, read and sha-1 hash it
                byte[] sentryFiledata = File.ReadAllBytes(SentryFile);
                sentryHash = CryptoHelper.SHAHash(sentryFiledata);
                LoginData.SentryFileHash = sentryHash; //We'll save the hash in the login data
            }

            Login(LoginData); //Lets login and send the login data we've been preparing now that we've connected to steam 

        }
        /// <summary>
        /// The callback that is fired when going online once connected to steam, we use it to tell the 
        /// userhandler class to run its login method
        /// </summary>
        /// <param name="callback"></param>
        void OnFriendsList(SteamFriends.FriendsListCallback callback)
        {
            // at this point, the client has received it's friends list
            Console.WriteLine("Steam Logged in");

            AnnounceLoginCompleted();

            foreach (ChatroomEntity Chatroom in GetMainChatroomsCollection())
            {
                SteamFriends.JoinChat(new SteamID(ulong.Parse(Chatroom.identifier.ToString())));
            }

        }
        /// <summary>
        /// Now that we're logged in, lets go online so we can properly interact on steamfriends.
        /// If there is difficulty interacting with people, it is likely because you're not online
        /// </summary>
        /// <param name="callback"></param>

        void OnAccountInfo(SteamUser.AccountInfoCallback callback)
        {
            // before being able to interact with friends, you must wait for the account info callback
            // this callback is posted shortly after a successful logon

            // at this point, we can go online on friends, so lets do that
            SteamFriends.SetPersonaState(EPersonaState.Online); //Set the State to Online
            
        }
        /// <summary>
        /// A method to Login using the given Data
        /// </summary>
        /// <param name="LoginDetails"></param>
        void Login(SteamUser.LogOnDetails LoginDetails)
        {
            steamUser.LogOn(LoginDetails);
        }
        /// <summary>
        /// This callback fires when steam tells us we've been disconnected, currently the 
        /// bot will run the reconnect method
        /// </summary>
        /// <param name="callback"></param>
        void OnDisconnected(SteamClient.DisconnectedCallback callback)
        {
            Console.WriteLine("Disconnected from , UserSetup: {0}", callback.UserInitiated);
            Reconnect();
        }

        /// <summary>
        /// This method should be used when we need to reconnect SteamClient, it'll allow us to keep things more organised 
        /// </summary>
        void Reconnect()
        {
            SteamDirectory.Initialize().Wait(15); //Update internal list that is heavily used before attempting login. And wait 2 Seconds to avoid Spam
            steamClient.Connect(); //Lets try and log back in
        }
        /// <summary>
        /// This is fired when steam gives the login key, so we store it
        /// </summary>
        /// <param name="callback"></param>
        void OnLoginKey(SteamUser.LoginKeyCallback callback)
        {
            if ((callback != null) && (!string.IsNullOrEmpty(callback.LoginKey))) //Check if the data is valid
            {
                steamUser.AcceptNewLoginKey(callback); //Tell steam we have accepted their offering
                File.WriteAllText(LoginKeyFile, callback.LoginKey); //We now will write the data 
                LoginData.LoginKey = callback.LoginKey; //We will now set the login Key used in Log-in
                Console.WriteLine("Wrote New LoginKey"); //Tell the user we have wrote a new login key file
            }
        }
        /// <summary>
        /// The Callback fired when attempting to logon
        /// </summary>
        /// <param name="callback"></param>
        void OnLoggedOn(SteamUser.LoggedOnCallback callback)
        {
            bool isSteamGuard = callback.Result == EResult.AccountLogonDenied; //Setup a bool to see if steamguard blocked the login
            bool is2FA = callback.Result == EResult.AccountLoginDeniedNeedTwoFactor; //Setup a bool to see if two factor auth blocked the login

            if (callback.Result != EResult.OK) //If we didn't log in
            {
                if (isSteamGuard || is2FA) //Check If steamguard is what stopped us  
                {
                    Console.WriteLine("This account is SteamGuard protected!");

                    if (is2FA) //Check if 2FA is what stopped us
                    {
                        Console.Write("Please enter your 2 factor auth code from your authenticator app: ");
                        LoginData.TwoFactorCode = Console.ReadLine(); //The user types the code they received and we set it for the next login attempt
                    }
                    else
                    {
                        Console.Write("Please enter the auth code sent to the email at {0}: ", callback.EmailDomain);
                        LoginData.AuthCode = Console.ReadLine(); //The user types the code they received and we set it for the next login attempt
                    }

                    return; //We return, which will lead to the ConnectionHandler attempting another login
                }
                else //If we didn't login but because of steamguard
                {
                    Console.WriteLine("Unable to logon to Steam: {0} / {1}", callback.Result, callback.ExtendedResult); //Tell the error
                    Console.WriteLine("{0} {1} This error is more indicative of an incorrect username + password or perhaps an invalid login key", ID, LoginData.Username); //Warn the user
                    //Often we can get denied login due to a bad login Key, so we'll switch to using passwords instead
                    //TODO add an IF argument to check if that was the reason, and add a delay


                    LoginData.LoginKey = null; //We will set the login Key to Null to tell steam we no longer want to use that method  
                    LoginData.Password = pass; //We set the Password as this is how we're verifying now 

                    return; //We go back now and try again
                }
            }
            else
            {
                Console.WriteLine("{0} Successfully logged on!", LoginData.Username); //Lets tell the user we logged on               
            }
        }
        /// <summary>
        /// This callback fires when we receive data for the sentry file, so we save it 
        /// </summary>
        /// <param name="callback"></param>
        void OnMachineAuth(SteamUser.UpdateMachineAuthCallback callback)
        {
            Console.WriteLine("Updating sentryfile...");

            // write out our sentry file
            // ideally we'd want to write to the filename specified in the callback
            // but then this sample would require more code to find the correct sentry file to read during logon
            // for the sake of simplicity, we'll just use "sentry.bin"

            int fileSize;
            byte[] sentryHash;
            using (var fs = File.Open(SentryFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
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
        //TODO Lets Turn the following four into events, so errors aren't thrown if a userhandler doesn't exist 


        /// <summary>
        /// Callback fires when we log out
        /// </summary>
        /// <param name="callback"></param>
        void OnLoggedOff(SteamUser.LoggedOffCallback callback)
        {
            Console.WriteLine("Logged off of Steam: {0}", callback.Result);
        }

        void ReceivePrivateMessage(SteamFriends.FriendMsgCallback callback)
        {
            MessageEventArgs NewMessageData = new MessageEventArgs(this);
            NewMessageData.ReceivedMessage = callback.Message;
            NewMessageData.Sender = new ChatroomEntity(callback.Sender,this);
            NewMessageData.Destination = NewMessageData.Sender;
            NewMessageData.Sender.DisplayName = SteamFriends.GetFriendPersonaName(callback.Sender);
            base.PrivateMessageProcessEvent(NewMessageData);

        }

        void ReceiveChatMessage(SteamFriends.ChatMsgCallback callback)
        {
          //  if (CheckEntryValid(callback.ChatRoomID.ConvertToUInt64().ToString())) //TODO get this code working
          //  { }

            MessageEventArgs NewMessageData = new MessageEventArgs(this);
            NewMessageData.ReceivedMessage = callback.Message;

            NewMessageData.Chatroom = new Chatroom(callback.ChatRoomID, this);

            NewMessageData.Sender = new User(callback.ChatterID, this);
            NewMessageData.Sender.DisplayName = SteamFriends.GetFriendPersonaName(callback.ChatterID);

            NewMessageData.Destination = NewMessageData.Chatroom;

            NewMessageData.Sender.DisplayName = SteamFriends.GetFriendPersonaName(callback.ChatterID);

            base.ChatRoomMessageProcessEvent(NewMessageData);


        }

        void Chatmemberinfo(SteamFriends.ChatMemberInfoCallback callback)
        {
            
            if (callback.StateChangeInfo.StateChange != EChatMemberStateChange.Entered)
            {

            }
            else
            {
                if (callback.StateChangeInfo.MemberInfo.Permissions.HasFlag(EChatPermission.MemberDefault))
                {
                    ChatMemberInfoProcessEvent(new ChatroomEntity(callback.StateChangeInfo.ChatterActedOn,this), true);
                }
                else
                {
                    ChatMemberInfoProcessEvent(new ChatroomEntity(callback.StateChangeInfo.ChatterActedOn,this), false);
                }
            }
        }

        public override void SendChatRoomMessage(object sender, MessageEventArgs messagedata)
        {
            try
            {
                SteamFriends.SendChatRoomMessage((SteamID)(messagedata.Destination.identifier), EChatEntryType.ChatMsg, messagedata.ReplyMessage);
            }
            catch
            {
                
            }
        }

        public override void SendPrivateMessage(object sender, MessageEventArgs messagedata)
        {
            try
            {
                SteamID user = new SteamID(messagedata.Destination.identifier.ToString());
                SteamFriends.SendChatMessage(user, EChatEntryType.ChatMsg, messagedata.ReplyMessage);
            }
            catch
            {

            }
        }

        public override void BroadCastMessage(object sender, string message)
        {
            foreach (ChatroomEntity Chatroom in GetMainChatroomsCollection())
            {
                SteamFriends.SendChatRoomMessage(new SteamID((ulong.Parse(Chatroom.identifier.ToString()))), EChatEntryType.ChatMsg, message);
            }
        }

        public override void EnterChatRoom(object sender, ChatroomEntity ChatroomEntity)
        {
            ulong item = ulong.Parse(ChatroomEntity.identifier.ToString());
            SteamFriends.JoinChat(new SteamID(item));
        }

        public override void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus)
        {
            throw new NotImplementedException();
        }

        public override void LeaveChatroom(object sender, ChatroomEntity ChatroomEntity)
        {
            ulong item = ulong.Parse(ChatroomEntity.identifier.ToString());
            SteamFriends.LeaveChat(new SteamID(item));
        }

        public override void SetUsername(object sender, string Username)
        {
            SteamFriends.SetPersonaName(Username);
        }

        public override string GetOthersUsername(object sender, ChatroomEntity user)
        {
            return SteamFriends.GetFriendPersonaName((SteamID)user.identifier);
        }


        public override void Reboot(object sender , EventArgs e)
        {
            ResetConnection(SteamBotLiteLoginData);
            Console.WriteLine("Rebooting");
        }

        public override string GetUsername()
        {
            return SteamFriends.GetPersonaName();
        }

        public override void SetStatusMessage(object sender, string message)
        {

            var request = new ClientMsgProtobuf<CMsgClientGamesPlayed>(EMsg.ClientGamesPlayed);

            var gamePlayed = new CMsgClientGamesPlayed.GamePlayed();

            if (!string.IsNullOrEmpty(message))
            {
                gamePlayed.game_id = 12350489788975939584;
                gamePlayed.game_extra_info = message;
            }

            request.Body.games_played.Add(gamePlayed);
            
            steamClient.Send(request);
        }


    }
}
