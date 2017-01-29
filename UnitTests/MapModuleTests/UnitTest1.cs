using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamBotLite;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace MapModuleTests
{
    [TestClass]
    public class UnitTest1
    {
        Dictionary<string, object> MakeConfig ()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();


            Dictionary<string, object> MapModuleConfig = new Dictionary<string, object>();
            MapModuleConfig.Add("ServerMapListUrl", new ObservableCollection<MapModule.Map>());
            MapModuleConfig.Add("MaxMapList", 5);

            dictionary.Add("MapModule",MapModuleConfig);

            return MapModuleConfig;
        }

        

        [TestMethod]
        public void RegularSyntax()
        {
            MapModule module = new MapModule(new TestUserHandler(), MakeConfig());
            
            string identifier = "0";
            ChatroomEntity user = new ChatroomEntity();
            user.identifier = identifier;


            MessageEventArgs Message = new MessageEventArgs(null);
            string command = "!add";
            string Mapname = "mapname";
            string url = "http://URL";
            string notes = "these are notes";
            Message.ReceivedMessage = command + " " + Mapname + " " + url + " " + notes;

            Message.Sender = user;

            foreach (BaseCommand c in module.commands)
            {
                if (c.CheckCommandExists(Message, Message.ReceivedMessage))
                {
                    c.run(Message, Message.ReceivedMessage);
                }
            }

            MapModule.Map TestMap = module.mapList[0];

            Assert.AreEqual(TestMap.Filename, Mapname);
            Assert.AreEqual(TestMap.DownloadURL, url);
            Assert.AreEqual(TestMap.Notes, notes);
            Assert.AreEqual(TestMap.Submitter, identifier);

            Assert.AreNotEqual(TestMap.Filename, Mapname + 1); //Ensure that its a string check
        }
    }
}
