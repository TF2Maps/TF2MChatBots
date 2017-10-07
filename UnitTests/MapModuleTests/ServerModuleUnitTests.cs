using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamBotLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamBotLite;

namespace UsersModuleTests
{
    [TestClass()]
    public class ServerTrackingModuleUnitTests
    {


        ServerTrackingModule module;
        string ServerAddCommand = "!ServerAdd";
        string TestIP = "192.168.0.1";
        string TestPort = "27015";
        string TestName = "TestServer";
        ChatroomEntity TestUser;

        public string ServerRemoveCommand = "!serverremove";

        Dictionary<string, Dictionary<string, object>> MakeConfig() {
            Dictionary<string, Dictionary<string, object>> ModuleHolder = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> ServerTrackingModuleData = new Dictionary<string, object>();
            ServerTrackingModuleData.Add("updateInterval", "999999");
            ModuleHolder.Add("ServerTrackingModule", ServerTrackingModuleData);

            return ModuleHolder;
        }


        [TestInitialize()]
        public void Initialize() {
            module = new ServerTrackingModule(new TestUserHandler(), new TestUserHandler() , MakeConfig());
            TestUser = new User(0, null);
            TestUser.Rank = ChatroomEntity.AdminStatus.True;
        }

        [TestCleanup()]
        public void Cleanup() {
            module = new ServerTrackingModule(new TestUserHandler(), new TestUserHandler(),  MakeConfig());
            module.TrackedServers.Clear();
            Assert.IsTrue(module.TrackedServers.Count() == 0);
        }

        string FireCommand(MessageEventArgs Message, BaseModule module)
        {
            string param = " ";

            foreach (BaseCommand c in module.commands)
            {
                if (c.CheckCommandExists(Message, Message.ReceivedMessage))
                {
                    param = c.run(Message, Message.ReceivedMessage);
                }
            }
            if (Message.Sender.Rank == ChatroomEntity.AdminStatus.True)
            {
                foreach (BaseCommand c in module.adminCommands)
                {
                    if (c.CheckCommandExists(Message, Message.ReceivedMessage))
                    {
                        param = c.run(Message, Message.ReceivedMessage);
                    }
                }
            }

            return param;
        }

        [TestMethod()]
        public void AddServer() {
            string message = ServerAddCommand + " " + TestName + " " + TestIP + " " + TestPort;

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = message;
            Message.Sender = TestUser;
            Console.WriteLine(FireCommand(Message, module));

            Console.WriteLine(module.TrackedServers.Servers[0].serverIP);

            Assert.IsTrue(module.TrackedServers.Count() == 1);

            Assert.IsTrue(module.TrackedServers.Servers[0].tag.Equals(TestName));
            Assert.IsTrue(module.TrackedServers.Servers[0].port.Equals(int.Parse(TestPort)));
            Assert.IsTrue(module.TrackedServers.Servers[0].serverIP.Equals(TestIP));
            Assert.IsTrue(ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        [TestMethod()]
        public void RejectMissingIP()
        {
            string message = ServerAddCommand + " " + TestName + " " ;

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = message;
            Message.Sender = TestUser;
            Console.WriteLine(FireCommand(Message, module));

            Assert.IsTrue(module.TrackedServers.Count() == 0);

        }

        [TestMethod()]
        public void RejectMissingPort()
        {
            string message = ServerAddCommand + " " + TestName + " " + TestIP + " ";

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = message;
            Message.Sender = TestUser;
            Console.WriteLine(FireCommand(Message, module));

            Assert.IsTrue(module.TrackedServers.Count() == 0);

        }

        [TestMethod()]
        public void Persistency() {
            AddServer();
            module = new ServerTrackingModule(new TestUserHandler(), new TestUserHandler(),  MakeConfig());

            Assert.IsTrue(module.TrackedServers.Count() == 1);

            Assert.IsTrue(module.TrackedServers.Servers[0].tag.Equals(TestName));
            Assert.IsTrue(module.TrackedServers.Servers[0].port.Equals(int.Parse(TestPort)));
            Assert.IsTrue(module.TrackedServers.Servers[0].serverIP.Equals(TestIP));
            Assert.IsTrue(ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        [TestMethod()]
        public void RemoveServerAfterAdd() {
            AddServer();
            string message = ServerRemoveCommand + " " + TestName;

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = message;
            Message.Sender = TestUser;

            Console.WriteLine(FireCommand(Message, module));

            Assert.IsTrue(module.TrackedServers.Count() == 0);

            Assert.IsFalse  (ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        [TestMethod()]
        public void CheckPersistencyOfRemove() {
            RemoveServerAfterAdd();
            module = new ServerTrackingModule(new TestUserHandler(), new TestUserHandler(), MakeConfig());
            Assert.IsTrue(module.TrackedServers.Count() == 0);

            Assert.IsFalse  (ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        [TestMethod()]
        public void ClearPersistency() {
            AddServer();
            module.TrackedServers.Clear();
            module = new ServerTrackingModule(new TestUserHandler(), new TestUserHandler(), MakeConfig());
            Assert.IsTrue(module.TrackedServers.Count() == 0);

            Assert.IsFalse  (ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        TrackingServerInfo TestTrackingServerInfo ()
        {
            TrackingServerInfo TestTrackingServerInfo = new TrackingServerInfo(TestIP, int.Parse(TestPort), TestName);
            return TestTrackingServerInfo;
        }
        string testmap = "koth_badlands";
        int testcapacity = 24;
        int testplayercount = 10;

        [TestMethod()]
        public void CheckQueryUpdatesOnAllChange()
        {
            string NewMapName = testmap + "_test";
            int NewCapacity = testcapacity + 1;
            int NewPlayercount = testplayercount + 1;
            AddServer();

            TrackingServerInfo TestServer = TestTrackingServerInfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncTrackingServerInfo(null, null);

            TestServer.currentMap = NewMapName;
            TestServer.capacity = NewCapacity;
            TestServer.playerCount = NewPlayercount;

            module.EmulateServerQuery(TestServer);
            module.SyncTrackingServerInfo(null, null);

            Assert.IsTrue(module.TrackedServers.Servers[0].currentMap.Equals(NewMapName));
            Assert.IsTrue(module.TrackedServers.Servers[0].playerCount.Equals(NewPlayercount));
            Assert.IsTrue(module.TrackedServers.Servers[0].capacity.Equals(NewCapacity));
        }

        [TestMethod()]
        public void CheckQueryUpdatesOnCapacityChange()
        {
            int NewCapacity = testcapacity + 1;
            int NewPlayercount = testplayercount + 1;
            AddServer();

            TrackingServerInfo TestServer = TestTrackingServerInfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncTrackingServerInfo(null, null);

            TestServer.capacity = NewCapacity;

            module.EmulateServerQuery(TestServer);
            module.SyncTrackingServerInfo(null, null);

            Assert.IsTrue(module.TrackedServers.Servers[0].capacity.Equals(NewCapacity));
        }

        [TestMethod()]
        public void CheckQueryUpdatesOnPlayerCountChange()
        {
            int NewCapacity = testcapacity + 1;
            int NewPlayercount = testplayercount + 1;
            AddServer();

            TrackingServerInfo TestServer = TestTrackingServerInfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncTrackingServerInfo(null, null);

            TestServer.playerCount = NewPlayercount;

            module.EmulateServerQuery(TestServer);
            module.SyncTrackingServerInfo(null, null);

            Assert.IsTrue(module.TrackedServers.Servers[0].playerCount.Equals(NewPlayercount));
        }

        [TestMethod()]
        public void CheckQueryUpdatesOnPlayerMapChange()
        {
            string NewMapName = testmap + "_test";

            AddServer();

            TrackingServerInfo TestServer = TestTrackingServerInfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncTrackingServerInfo(null, null);

            TestServer.currentMap = NewMapName;

            module.EmulateServerQuery(TestServer);
            module.SyncTrackingServerInfo(null, null);

            Assert.IsTrue(module.TrackedServers.Servers[0].currentMap.Equals(NewMapName));
        }

        bool ContainsCommand(string commandname , List<BaseCommand> commands) {
            foreach(BaseCommand command in commands)
            {
                if (command.CheckCommandExists(null, commandname))
                {
                    return true;
                }
            }
            return false;
        }
      

    }
}