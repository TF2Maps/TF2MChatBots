using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamBotLite;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MapModuleTests
{

    abstract class MapModuleTest
    {
        public MapModuleTest()
        {
            
        }
    }
    [TestClass]
    public class UnitTest1 
    {
        string command = "!add";
        string Mapname = "mapname";
        string url = "http://URL";
        string notes = "these are notes";
        MapModule module;

        string identifier = "0";
        ChatroomEntity TestUser;
        

        public UnitTest1 () {
            TestUser = new ChatroomEntity();
            TestUser.identifier = identifier;
            module = new MapModule(new TestUserHandler(), MakeConfig());
        }

        Dictionary<string, object> MakeConfig()
        {
            Dictionary<string, object> MapModuleConfig = new Dictionary<string, object>();

            MapModuleConfig.Add("ServerMapListUrl", new ObservableCollection<MapModule.Map>());
            MapModuleConfig.Add("MaxMapList", 5);
            
            return MapModuleConfig;
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void Initialize()
        {
        //    module = new MapModule(new TestUserHandler(), MakeConfig());
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void Cleanup()
        {
            module.ClearMapListWithMessage("Test Wipe");
            Assert.IsTrue(module.mapList.Count == 0);
        }



        [TestMethod]
        public void RegularSyntax()
        {
            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = command  +" "+  Mapname  +" "+  url +" "+  notes;
            Message.Sender = TestUser;

            FireCommand(Message , module);

            MapModule.Map TestMap = module.mapList[0];
            
            Assert.AreEqual(TestMap.Filename, Mapname);
            Assert.AreEqual(TestMap.DownloadURL, url);
            Assert.AreEqual(TestMap.Notes, notes);
            Assert.AreEqual(TestMap.Submitter, identifier);

            Assert.AreNotEqual(TestMap.Filename, Mapname + 1); //Ensure that its a string check
        }

        string FireCommand (MessageEventArgs Message , BaseModule module)
        {
            string param = " ";

            foreach (BaseCommand c in module.commands)
            {
                if (c.CheckCommandExists(Message, Message.ReceivedMessage))
                {
                    param = c.run(Message, Message.ReceivedMessage);
                }
            }
            return param;
        }

        [TestMethod]
        public void NoURL()
        {

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = command + " " + Mapname;
            Message.Sender = TestUser;

            FireCommand(Message, module);

            Assert.IsTrue(module.mapList.Count == 0);
        }

        [TestMethod]
        public void NoData()
        {

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = command;
            Message.Sender = TestUser;

            FireCommand(Message, module);

            Assert.IsTrue(module.mapList.Count == 0);
        }

        [TestMethod]
        public void ExtraSpacing()
        {

            MessageEventArgs Message = new MessageEventArgs(null);
            Message.ReceivedMessage = command + "   " + Mapname + "   " + url + "          " + notes;
            Message.Sender = TestUser;

            Console.WriteLine(FireCommand(Message, module));

            MapModule.Map TestMap = module.mapList[0];

            Assert.AreEqual(TestMap.Filename, Mapname);
            Assert.AreEqual(TestMap.DownloadURL, url);
            Assert.AreEqual(TestMap.Notes, notes);
            Assert.AreEqual(TestMap.Submitter, identifier);

            Assert.AreNotEqual(TestMap.Filename, Mapname + 1); //Ensure that its a string check
        }

    }
}
