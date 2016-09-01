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
        double interval = 60000;
        readonly int InitialGhostCheck = 10;
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
        ImpNaoModule impnaomodule;

        public UsersModule usersModule;

        public List<BaseModule> ModuleList;

        public List<BaseCommand> chatCommands = new List<BaseCommand>();
        public List<BaseCommand> chatAdminCommands = new List<BaseCommand>();

        // Loading Config
        Dictionary<string, object> jsconfig = JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(@"config.json"));

        /// <summary>
        /// Do not try using steamfriends, steamuser and all that since it'll be uninitialised at this point 
        /// </summary>
        /// <param name="SteamConnectionHandler"></param>
        public VBot(SteamConnectionHandler SteamConnectionHandler) : base(SteamConnectionHandler)
        {
            Console.WriteLine("Vbot Initialised");
            Console.WriteLine("Loading modules and stuff");
            
            GroupChatID = ulong.Parse((string)jsconfig["GroupchatID"]);
            GroupChatSID = new SteamID(GroupChatID);
            try {
                Autojoin = Convert.ToBoolean(jsconfig["AutoJoin"]);
            } catch { };
             
            // loading modules

            motdModule = new MotdModule(this, jsconfig);
            mapModule = new MapModule(this, jsconfig);
            serverModule = new ServerModule(this, jsconfig);
            usersModule = new UsersModule(this, jsconfig);
            replyModule = new RepliesModule(this, jsconfig);
            adminmodule = new AdminModule(this, jsconfig);
            searchModule = new SearchModule(this, jsconfig);


            ModuleList = new List<BaseModule> { motdModule,mapModule,serverModule,usersModule,replyModule,adminmodule,searchModule};

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
        public void Disablemodule(string ModuleToRemove)
        {
            int x = 0;
            int EntryToRemove = 0;
            bool RemoveModule = false;
            foreach (BaseModule Module in ModuleList)
            {
                Console.WriteLine(Module.GetType().ToString());
                if (Module.GetType().Name.ToString().Equals(ModuleToRemove))
                {
                    EntryToRemove = x;
                    RemoveModule = true;
                }
                x++;
            }
            if (RemoveModule && ModuleList[EntryToRemove].DeletableModule)
            {
                ModuleList[EntryToRemove] = null;
                ModuleList.RemoveAt(EntryToRemove);
            }
        }
        public void Enablemodule(string ModuleToAdd)
        {
            Type T = Type.GetType("SteamBotLite." + ModuleToAdd); //We attempt to translate the string to an existing type
            if ((T.GetType() != null) & (T.BaseType.ToString().Equals("SteamBotLite.BaseModule"))) //Then we check its valid AND if its a base of userhandler
            {                
                BaseModule module =  (BaseModule)Activator.CreateInstance(T, new object[] { this, jsconfig });
                bool AlreadyExists = false;

                foreach (BaseModule ExistingModule in ModuleList)
                {
                    if (ExistingModule.GetType() == module.GetType())
                    {
                        AlreadyExists = true;
                    }
                }
                if (!AlreadyExists)
                {
                    ModuleList.Add(module);
                }
            }
        }


        public string ChatMessageHandler(SteamID Sender , string Message)
        {
            string response = null;
            foreach (BaseModule module in ModuleList)
            {
                foreach (BaseCommand c in module.commands)
                    if (Message.StartsWith(c.command, StringComparison.OrdinalIgnoreCase))
                    {
                        response = c.run(Sender, Message);
                        return response;
                    }
            }

            if (usersModule.admincheck(Sender)) //Verifies that it is a moderator, Can you please check if the "ISAdmin" is being used correctly? 
            {
                Console.WriteLine("ADMIN SPOKE");
                foreach (BaseModule module in ModuleList)
                {
                    foreach (BaseCommand c in module.adminCommands)
                        if (Message.StartsWith(c.command, StringComparison.OrdinalIgnoreCase))
                        {
                            response = c.run(Sender, Message);
                            return response;
                        }
                }
            }
            return response;
        }
        public void OnMaplistchange(int MapCount, object sender = null, NotifyCollectionChangedEventArgs args = null)
        {
            Console.WriteLine("DOOT DODO DOOOOO");
            Console.WriteLine("Event Fired");

            steamConnectionHandler.SteamFriends.SetPersonaName("[" + MapCount + "]" + Username);            
        }
        public void ServerUpdated(object sender, ServerModule.ServerInfo args)
        {
            Console.WriteLine("Entered VBot");
            if (mapModule != null)
            {
                mapModule.HandleEvent(sender, args);
            }
        }


        public override void ChatMemberInfo(SteamFriends.ChatMemberInfoCallback callback)
        {
            if (callback.StateChangeInfo.StateChange != EChatMemberStateChange.Entered)
            {
                Console.WriteLine("User left chat");
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
            Console.WriteLine(string.Format("Ghostcheck = {0}"),GhostCheck);
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
