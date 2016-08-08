using System;
using SteamKit2;
using System.Timers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Specialized;

namespace SteamBotLite
{

    class VBot : UserHandler
    {
        ulong GroupChatID;
        public SteamID GroupChatSID;
        double interval = 5000;
        readonly int InitialGhostCheck = 480;
        int GhostCheck = 480;
        int CrashCheck = 0;
        public string Username = "V2Bot";
        Timer Tick;
        bool Autojoin = true; 

        // Class members
        MotdModule motdModule;
        MapModule mapModule;
        ServerModule serverModule;
        RepliesModule replyModule;
        AdminModule adminmodule;
        SearchModule searchModule;
        public UsersModule usersModule;

        List<BaseModule> ModuleList;

        public List<BaseCommand> chatCommands = new List<BaseCommand>();
        public List<BaseCommand> chatAdminCommands = new List<BaseCommand>();


        public VBot(SteamConnectionHandler SteamConnectionHandler) : base(SteamConnectionHandler)
        {
            Console.WriteLine("Vbot Initialised");
            Console.WriteLine("Loading modules and stuff");
            
            // Loading Config
            Dictionary<string, object> jsconfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(@"config.json"));
            GroupChatID = ulong.Parse((string)jsconfig["GroupchatID"]);
            GroupChatSID = new SteamID(GroupChatID);
            try {
                Autojoin = Convert.ToBoolean(jsconfig["AutoJoin"]);
            } catch { };
             
           
            // loading modules
            motdModule = new MotdModule(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(jsconfig["MotdModule"].ToString()));

            mapModule = new MapModule(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(jsconfig["MapModule"].ToString()));
            mapModule.mapList.CollectionChanged += OnMaplistchange;

            serverModule = new ServerModule(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(jsconfig["ServerModule"].ToString()));
            usersModule = new UsersModule(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(jsconfig["usersModule"].ToString()));
            replyModule = new RepliesModule(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(jsconfig["ReplyModule"].ToString()));
            adminmodule = new AdminModule(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(jsconfig["AdminModule"].ToString()));
            searchModule = new SearchModule(this, JsonConvert.DeserializeObject<Dictionary<string, object>>(jsconfig["AdminModule"].ToString()));

            ModuleList = new List<BaseModule> { motdModule,mapModule,serverModule,usersModule,replyModule,adminmodule,searchModule};

            serverModule.ServerUpdated += mapModule.HandleEvent;

            // loading module commands
            foreach (BaseModule module in ModuleList)
            {
                chatCommands.AddRange(module.commands);
                chatAdminCommands.AddRange(module.adminCommands);
            }
            Console.WriteLine("All Loaded");
        }

        public override void OnLoginCompleted()
        {
            steamConnectionHandler.SteamFriends.SetPersonaName("V2Bot");
            if (Autojoin)
                steamConnectionHandler.SteamFriends.JoinChat(GroupChatSID);
            InitTimer();
            Console.WriteLine("{0} User: {1} Is now online", steamConnectionHandler.ID, steamConnectionHandler.LoginData.Username); //Lets tell the User we're now online
        }

        public override void OnMessage(SteamFriends.FriendMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            string response = ChatMessageHandler(ChatMsg.Sender, ChatMsg.Message);
            if (response != null)
                steamConnectionHandler.SteamFriends.SendChatMessage(ChatMsg.Sender, EChatEntryType.ChatMsg, response);
        }
        public override void OnChatRoomMessage(SteamFriends.ChatMsgCallback ChatMsg) //This is an example of using older methods for cross-compatibility, by converting the new format to the older one
        {
            GhostCheck = InitialGhostCheck;
            CrashCheck = 0;
            string response = ChatMessageHandler(ChatMsg.ChatterID, ChatMsg.Message);
            if (response != null)
                steamConnectionHandler.SteamFriends.SendChatRoomMessage(GroupChatSID, EChatEntryType.ChatMsg, response);
        }

        public string ChatMessageHandler(SteamID Sender , string Message)
        {
            string response = null;

            foreach (BaseCommand c in chatCommands)
                if (Message.StartsWith(c.command, StringComparison.OrdinalIgnoreCase))
                    response = c.run(Sender, Message);

            if (usersModule.admincheck(Sender)) //Verifies that it is a moderator, Can you please check if the "ISAdmin" is being used correctly? 
            {
                Console.WriteLine("ADMIN SPOKE");
                foreach (BaseCommand c in chatAdminCommands)
                    if (Message.StartsWith(c.command, StringComparison.OrdinalIgnoreCase))
                        response = c.run(Sender, Message);
            }
            return response;
        }
        public void OnMaplistchange(object sender, NotifyCollectionChangedEventArgs args)
        {
            steamConnectionHandler.SteamFriends.SetPersonaName("[" + mapModule.mapList.Count + "]" + Username);            
        }

        public override void ChatMemberInfo(SteamFriends.ChatMemberInfoCallback callback)
        {
            if (callback.StateChangeInfo.StateChange == EChatMemberStateChange.Left)
            {
                return;
            }
            else
            {
                Console.WriteLine("Callback Fired");
                usersModule.updateUserInfo(callback);
            }
        }
        /// <summary>
        /// The Main Timer's method, executed per tick
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void TickTasks(object sender, EventArgs e)
        {
            GhostCheck--;

            if (GhostCheck <= 1)
            {
                GhostCheck = InitialGhostCheck;
                CrashCheck += 1;
                steamConnectionHandler.SteamFriends.LeaveChat(new SteamID(GroupChatSID));
                steamConnectionHandler.SteamFriends.JoinChat(new SteamID(GroupChatSID));
            }
            if (CrashCheck >= 4)
            {
                CrashCheck = 0; 
                Reboot();
            }
        }

        /// <summary>
        /// Initialises the main timer
        /// </summary>
        void InitTimer()
        {
            Tick = new Timer();
            Tick.Elapsed += new ElapsedEventHandler(TickTasks);
            Tick.Interval = interval; // in miliseconds
            Tick.Start();
        }

        public override void ClanStateCallback(SteamFriends.ClanStateCallback callback)
        {
            
        }
    }
}
