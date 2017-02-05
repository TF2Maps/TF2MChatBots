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
    public class ServerModuleUnitTests
    {


        ServerModule module;
        string ServerAddCommand = "!ServerAdd";
        string TestIP = "192.168.0.1";
        string TestPort = "27015";
        string TestName = "TestServer";
        ChatroomEntity TestUser;

        public string ServerRemoveCommand = "!serverremove";

        Dictionary<string, Dictionary<string, object>> MakeConfig() {
            Dictionary<string, Dictionary<string, object>> ModuleHolder = new Dictionary<string, Dictionary<string, object>>();
            Dictionary<string, object> ServerModuleData = new Dictionary<string, object>();
            ServerModuleData.Add("updateInterval", "999999");
            ModuleHolder.Add("ServerModule", ServerModuleData);

            return ModuleHolder;
        }


        [TestInitialize()]
        public void Initialize() {
            module = new ServerModule(new TestUserHandler(), MakeConfig());
            TestUser = new User(0, null);
            TestUser.Rank = ChatroomEntity.AdminStatus.True;
        }

        [TestCleanup()]
        public void Cleanup() {
            module = new ServerModule(new TestUserHandler(), MakeConfig());
            module.serverList.Clear();
            Assert.IsTrue(module.serverList.Count() == 0);
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

            Console.WriteLine(module.serverList.Servers[0].serverIP);

            Assert.IsTrue(module.serverList.Count() == 1);

            Assert.IsTrue(module.serverList.Servers[0].tag.Equals(TestName));
            Assert.IsTrue(module.serverList.Servers[0].port.Equals(int.Parse(TestPort)));
            Assert.IsTrue(module.serverList.Servers[0].serverIP.Equals(TestIP));
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

            Assert.IsTrue(module.serverList.Count() == 0);

        }

        [TestMethod()]
        public void RejectMissingPort()
        {
            string message = ServerAddCommand + " " + TestName + " " + TestIP + " ";

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = message;
            Message.Sender = TestUser;
            Console.WriteLine(FireCommand(Message, module));

            Assert.IsTrue(module.serverList.Count() == 0);

        }

        [TestMethod()]
        public void Persistency() {
            AddServer();
            module = new ServerModule(new TestUserHandler(), MakeConfig());

            Assert.IsTrue(module.serverList.Count() == 1);

            Assert.IsTrue(module.serverList.Servers[0].tag.Equals(TestName));
            Assert.IsTrue(module.serverList.Servers[0].port.Equals(int.Parse(TestPort)));
            Assert.IsTrue(module.serverList.Servers[0].serverIP.Equals(TestIP));
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

            Assert.IsTrue(module.serverList.Count() == 0);

            Assert.IsFalse  (ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        [TestMethod()]
        public void CheckPersistencyOfRemove() {
            RemoveServerAfterAdd();
            module = new ServerModule(new TestUserHandler(), MakeConfig());
            Assert.IsTrue(module.serverList.Count() == 0);

            Assert.IsFalse  (ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        [TestMethod()]
        public void ClearPersistency() {
            AddServer();
            module.serverList.Clear();
            module = new ServerModule(new TestUserHandler(), MakeConfig());
            Assert.IsTrue(module.serverList.Count() == 0);

            Assert.IsFalse  (ContainsCommand(module.NameToserverCommand(TestName), module.commands));
        }

        ServerInfo TestServerinfo ()
        {
            ServerInfo TestServerinfo = new ServerInfo(TestIP, int.Parse(TestPort), TestName);
            return TestServerinfo;
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

            ServerInfo TestServer = TestServerinfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncServerInfo(null, null);

            TestServer.currentMap = NewMapName;
            TestServer.capacity = NewCapacity;
            TestServer.playerCount = NewPlayercount;

            module.EmulateServerQuery(TestServer);
            module.SyncServerInfo(null, null);

            Assert.IsTrue(module.serverList.Servers[0].currentMap.Equals(NewMapName));
            Assert.IsTrue(module.serverList.Servers[0].playerCount.Equals(NewPlayercount));
            Assert.IsTrue(module.serverList.Servers[0].capacity.Equals(NewCapacity));
        }

        [TestMethod()]
        public void CheckQueryUpdatesOnCapacityChange()
        {
            int NewCapacity = testcapacity + 1;
            int NewPlayercount = testplayercount + 1;
            AddServer();

            ServerInfo TestServer = TestServerinfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncServerInfo(null, null);

            TestServer.capacity = NewCapacity;

            module.EmulateServerQuery(TestServer);
            module.SyncServerInfo(null, null);

            Assert.IsTrue(module.serverList.Servers[0].capacity.Equals(NewCapacity));
        }

        [TestMethod()]
        public void CheckQueryUpdatesOnPlayerCountChange()
        {
            int NewCapacity = testcapacity + 1;
            int NewPlayercount = testplayercount + 1;
            AddServer();

            ServerInfo TestServer = TestServerinfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncServerInfo(null, null);

            TestServer.playerCount = NewPlayercount;

            module.EmulateServerQuery(TestServer);
            module.SyncServerInfo(null, null);

            Assert.IsTrue(module.serverList.Servers[0].playerCount.Equals(NewPlayercount));
        }

        [TestMethod()]
        public void CheckQueryUpdatesOnPlayerMapChange()
        {
            string NewMapName = testmap + "_test";

            AddServer();

            ServerInfo TestServer = TestServerinfo();

            TestServer.currentMap = testmap;
            TestServer.capacity = testcapacity;
            TestServer.playerCount = testplayercount;

            module.EmulateServerQuery(TestServer);

            module.SyncServerInfo(null, null);

            TestServer.currentMap = NewMapName;

            module.EmulateServerQuery(TestServer);
            module.SyncServerInfo(null, null);

            Assert.IsTrue(module.serverList.Servers[0].currentMap.Equals(NewMapName));
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