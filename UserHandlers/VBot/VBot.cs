using System;
using System.Timers;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Collections.Specialized;
using System.Collections.ObjectModel;

namespace SteamBotLite
{
    public class VBot : UserHandler, HTMLFileFromArrayListiners, ModuleHandler
    {
        
        bool Autojoin = true; 

        // Class members
        MotdModule motdModule;
        public MapModule mapModule;
        ServerModule serverModule;
        RepliesModule replyModule;
        AdminModule adminmodule;
        SearchModule searchModule;
        ImpNaoModule impnaomodule;
        ServerListHolder serverlistmodule;
        CountDownModule countdownmodule;
        MapWebServer WebServer;
        IdentityModule identitymodule;

        public UsersModule usersModule;

        public List<BaseModule> ModuleList;

        public List<HTMLFileFromArrayListiners> HTMLParsers;

        List<BaseCommand> chatCommands = new List<BaseCommand>();
        List<BaseCommand> chatAdminCommands = new List<BaseCommand>();

        public List<MapListChangeListiner> ListChangeEventListiners = new List<MapListChangeListiner>();

        public List<ServerMapChangeListiner> MapChangeEventListiners = new List<ServerMapChangeListiner>();
        // Loading Config
        Dictionary<string, Dictionary<string,object>> jsconfig = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string,object>>>(System.IO.File.ReadAllText(@"config.json"));

        /// <summary>
        /// Do not try using steamfriends, steamuser and all that since it'll be uninitialised at this point 
        /// </summary>
        /// <param name="SteamConnectionHandler"></param>
        public VBot() 
        {

           // base.SetUsernameEvent += UpdateUsernameEvent;
            Console.WriteLine("VBot Initialised");
            Console.WriteLine("Loading modules and stuff");

            MapChangeEventListiners = new List<ServerMapChangeListiner>();
            HTMLParsers = new List<HTMLFileFromArrayListiners>();
            OnLoginlistiners = new List<OnLoginCompletedListiners>();
            ListChangeEventListiners = new List<MapListChangeListiner>();
            // loading modules
            WebServer = new MapWebServer(this, jsconfig);

            mapModule = new MapModule(this,this, jsconfig);

            serverlistmodule = new ServerListHolder(this, this, jsconfig);
            motdModule = new MotdModule(this, jsconfig);
            serverModule = new ServerModule(this, this, jsconfig);
            usersModule = new UsersModule(this, jsconfig);
            replyModule = new RepliesModule(this, jsconfig);
            searchModule = new SearchModule(this, jsconfig);
            adminmodule = new AdminModule(this,this, jsconfig);
            identitymodule = new IdentityModule(this, this, jsconfig);
            countdownmodule = new CountDownModule(this, jsconfig);

            ModuleList = new List<BaseModule> { motdModule,mapModule,serverModule,identitymodule , usersModule,replyModule,adminmodule,searchModule, WebServer, serverlistmodule , countdownmodule };

            Console.WriteLine("Modules loaded and ModuleList intitialised");

            foreach (BaseModule module in ModuleList)
            {
                module.OnAllModulesLoaded();
            }

          //  OnMaplistchange(mapModule.mapList);
        }

        
        public void UpdateUsernameEvent(object sender, string e)
        {
            base.SetUsernameEventProcess(e);
        }

        
        

        public void OnMaplistchange(IReadOnlyList<Map> maplist, object sender = null, NotifyCollectionChangedEventArgs args = null)
        {
            foreach (MapListChangeListiner listiner in ListChangeEventListiners)
            {
                listiner.MaplistChange(maplist);
            }
           

        }

        public void AddListChangeEventListiner(MapListChangeListiner listiner)
        {
            ListChangeEventListiners.Add(listiner);
        }

        public List<OnLoginCompletedListiners> OnLoginlistiners;

        public override void OnLoginCompleted(object sender, EventArgs e)
        {
            if (Autojoin)
                {
                    base.FireMainChatRoomEvent(ChatroomEventEnum.EnterChat);
                }
            foreach (OnLoginCompletedListiners listiner in OnLoginlistiners) {
                listiner.OnLoginCompleted();
            }
            Console.WriteLine("UserHandler: {0} Has Loaded", this.GetType());
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

        public override void ProcessChatRoomMessage(object sender, MessageEventArgs e)
        {
            e.ReplyMessage = ChatMessageHandler(e, e.ReceivedMessage);
            if (e.ReplyMessage != null)
            {
                e.InterfaceHandlerDestination.SendChatRoomMessage(this, e);
            }
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


        public string ChatMessageHandler(MessageEventArgs Msg , string Message)
        {
            string response = null;
            foreach (BaseModule module in ModuleList)
            {
                if (module != null)
                {
                    foreach (BaseCommand c in module.commands)
                    {
                        if (c.CheckCommandExists(Msg,Message))
                        {
                            response = c.run(Msg, Message);
                            return response;
                        }
                    }
                    
                }
            }

            if (usersModule.admincheck(Msg.Sender)) //Verifies that it is a moderator, Can you please check if the "ISAdmin" is being used correctly? 
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
        

        public void ServerUpdated(object sender, ServerInfo args)
        {
            if (MapChangeEventListiners.Count > 0 )
            {
                foreach(ServerMapChangeListiner Listiner in MapChangeEventListiners)
                {
                    Listiner.OnMapChange(args);
                }
            }
        }

        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity,bool>e)
        {
            usersModule.updateUserInfo(e.Item1, e.Item2);
        }

        public void HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey)
        {
            foreach(HTMLFileFromArrayListiners Listiner in HTMLParsers)
            {
                Listiner.HTMLFileFromArray(Headernames, Data, TableKey);
            }
        }

        public bool admincheck(ChatroomEntity user)
        {
            return usersModule.admincheck(user);
        }

        public void AddMapChangeEventListiner(ServerMapChangeListiner listiner)
        {
            MapChangeEventListiners.Add(listiner);
        }

        public void AddLoginEventListiner(OnLoginCompletedListiners listiner)
        {
            OnLoginlistiners.Add(listiner);
        }

        

        public List<BaseModule> GetAllModules()
        {
            return ModuleList;
        }

        public void AddHTMLTable(string TableKey, string Tabledata)
        {
            foreach (HTMLFileFromArrayListiners Listiner in HTMLParsers)
            {
                Listiner.AddHTMLTable(TableKey,Tabledata);
            }
        }

        public void MakeTableFromEntry(string TableKey, TableData TableData)
        {
            foreach (HTMLFileFromArrayListiners Listiner in HTMLParsers)
            {
                Listiner.MakeTableFromEntry(TableKey, TableData);
            }
        }

        public void AddEntryWithoutLimit(string identifier, TableDataValue[] data)
        {
            foreach (HTMLFileFromArrayListiners Listiner in HTMLParsers)
            {
                Listiner.AddEntryWithoutLimit(identifier, data);
            }
        }

        public void AddEntryWithLimit(string identifier, TableDataValue[] data, int limit)
        {
            foreach (HTMLFileFromArrayListiners Listiner in HTMLParsers)
            {
                Listiner.AddEntryWithLimit(identifier, data , limit);
            }
        }

        public void SetTableHeader(string TableIdentifier, TableDataValue[] Header)
        {
            foreach (HTMLFileFromArrayListiners Listiner in HTMLParsers)
            {
                Listiner.SetTableHeader(TableIdentifier, Header);
            }
        }
    }
}
