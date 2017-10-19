using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SteamBotLite
{
    public class VBot : UserHandler, IHTMLFileFromArrayPasser, ModuleHandler
    {
        public List<IHTMLFileFromArrayPasser> HTMLParsers;



        public List<MapListChangeListiner> ListChangeEventListiners = new List<MapListChangeListiner>();
        public List<ServerMapChangeListiner> MapChangeEventListiners = new List<ServerMapChangeListiner>();
        public MapModule mapModule;
        public List<BaseModule> ModuleList;
        public List<OnLoginCompletedListiners> OnLoginlistiners;
        public UsersModule usersModule;
        private AdminModule adminmodule;
        private bool Autojoin = true;

        private List<BaseCommand> chatAdminCommands = new List<BaseCommand>();

        private List<BaseCommand> chatCommands = new List<BaseCommand>();

        private CountDownModule countdownmodule;

        private IdentityModule identitymodule;

        // Loading Config
        private Dictionary<string, Dictionary<string, object>> jsconfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(System.IO.File.ReadAllText(@"config.json"));

        // Class members
        private MotdModule motdModule;

        private RepliesModule replyModule;
        private SearchModule searchModule;
        private ServerTrackingModule ServerTrackingModule;
        private TrackingServerListHolder TrackingServerListmodule;
        private WebServerHostingModule WebServer;

        /// <summary>
        /// Do not try using steamfriends, steamuser and all that since it'll be uninitialised at this point
        /// </summary>
        /// <param name="SteamConnectionHandler"></param>
        public VBot()
        {
            
            Console.WriteLine("VBot Initialised");
            Console.WriteLine("Loading modules and stuff");

            MapChangeEventListiners = new List<ServerMapChangeListiner>();
            HTMLParsers = new List<IHTMLFileFromArrayPasser>();
            OnLoginlistiners = new List<OnLoginCompletedListiners>();
            ListChangeEventListiners = new List<MapListChangeListiner>();
            ModuleList = new List<BaseModule>();
            // loading modules
            WebServer = new WebServerHostingModule(this, jsconfig);
            mapModule = new MapModule(this, this, jsconfig);
            TrackingServerListmodule = new TrackingServerListHolder(this, this, jsconfig);
            motdModule = new MotdModule(this, jsconfig);
            ServerTrackingModule = new ServerTrackingModule(this, this, jsconfig);
            usersModule = new UsersModule(this, jsconfig);
            replyModule = new RepliesModule(this, jsconfig);
            searchModule = new SearchModule(this, jsconfig);
            adminmodule = new AdminModule(this, this, this, jsconfig );
            identitymodule = new IdentityModule(this, this, jsconfig);
            countdownmodule = new CountDownModule(this, jsconfig);

            Console.WriteLine("Modules loaded and ModuleList intitialised");

            foreach (BaseModule module in ModuleList)
            {
                module.OnAllModulesLoaded();
            }
        }

        public void AddListChangeEventListiner(MapListChangeListiner listiner)
        {
            ListChangeEventListiners.Add(listiner);
        }

        public void AddLoginEventListiner(OnLoginCompletedListiners listiner)
        {
            OnLoginlistiners.Add(listiner);
        }

        public void AddModuleToCurrentModules(BaseModule module)
        {
            this.ModuleList.Add(module);
        }
        public void AddMapChangeEventListiner(ServerMapChangeListiner listiner)
        {
            MapChangeEventListiners.Add(listiner);
        }

        public bool admincheck(ChatroomEntity user)
        {
            return usersModule.admincheck(user);
        }

        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e)
        {
            usersModule.updateUserInfo(e.Item1, e.Item2);
        }

        public string ChatMessageHandler(MessageEventArgs Msg, string Message)
        {
            string response = null;

            foreach (BaseModule module in ModuleList)
            {
                if (module != null)
                {
                    foreach (BaseCommand c in module.commands)
                    {
                        if (c.CheckCommandExists(Msg, Message))
                        {
                            response = c.run(Msg, Message);
                            return response;
                        }
                    }
                }
            }

            if (usersModule.admincheck(Msg.Sender))
            {
                foreach (BaseModule module in ModuleList)
                {
                    if (module != null)
                    {
                        foreach (BaseCommand c in module.adminCommands)
                            if (c.CheckCommandExists(Msg, Message))
                            {
                                response = c.run(Msg, Message);
                                return response;
                            }
                    }
                }
            }
            return response;
        }

        public List<BaseModule> GetAllModules()
        {
            return ModuleList;
        }

        public override void OnLoginCompleted(object sender, EventArgs e)
        {
            if (Autojoin)
            {
                base.FireMainChatRoomEvent(ChatroomEventEnum.EnterChat);
            }
            foreach (OnLoginCompletedListiners listiner in OnLoginlistiners)
            {
                listiner.OnLoginCompleted();
            }

            Console.WriteLine("UserHandler: {0} Has Loaded", this.GetType());
        }

        public void OnMaplistchange(IReadOnlyList<Map> maplist, object sender = null, NotifyCollectionChangedEventArgs args = null)
        {
            foreach (MapListChangeListiner listiner in ListChangeEventListiners)
            {
                listiner.MaplistChange(maplist);
            }
        }

        public override void ProcessChatRoomMessage(object sender, MessageEventArgs e)
        {
            e.ReplyMessage = ChatMessageHandler(e, e.ReceivedMessage);
            if (e.ReplyMessage != null)
            {
                e.InterfaceHandlerDestination.SendChatRoomMessageAsync(this, e);
            }
        }

        public override void ProcessPrivateMessage(object sender, MessageEventArgs e)
        {
            ApplicationInterface AppInterface = (ApplicationInterface)sender;
            e.InterfaceHandlerDestination = AppInterface;
            e.ReplyMessage = ChatMessageHandler(e, e.ReceivedMessage);
            if (e.ReplyMessage != null)
            {
                e.InterfaceHandlerDestination.SendPrivateMessage(this, e);
            }
        }

        public void ServerUpdated(object sender, TrackingServerInfo args)
        {
            if (MapChangeEventListiners.Count > 0)
            {
                foreach (ServerMapChangeListiner Listiner in MapChangeEventListiners)
                {
                    Listiner.OnMapChange(args);
                }
            }
        }

        public void UpdateUsernameEvent(object sender, string e)
        {
            base.SetUsernameEventProcess(e);
        }

        public void HandleCommand(HTMLCommand command)
        {
            foreach (IHTMLFileFromArrayPasser Listiner in HTMLParsers)
            {
                Listiner.HandleCommand(command);
            }
        }
    }
}