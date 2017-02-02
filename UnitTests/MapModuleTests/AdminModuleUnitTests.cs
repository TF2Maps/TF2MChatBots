using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SteamBotLite;

namespace MapModuleTests
{
    [TestClass]
    public class AdminModuleUnitTests
    {
        AdminModule module;
        User TestUser;
        int identifier = 0;

        Dictionary<string, Dictionary<string, object>> MakeConfig()
        {

            Dictionary<string, Dictionary<string, object>> ModuleHolder = new Dictionary<string, Dictionary<string, object>>();

            Dictionary<string, object> ModuleConfig = new Dictionary<string, object>();
            ModuleConfig.Add("DefaultUsername", "USERNAME");
            ModuleConfig.Add("DefaultStatus", "STATUS");
            ModuleConfig.Add("UseStatus", "TRUE");

            ModuleHolder.Add("AdminModule", ModuleConfig);

            return ModuleHolder;
        }

        Dictionary<string, object> MakeConfig2()
        {

            Dictionary<string, object> ModuleConfig = new Dictionary<string, object>();
            ModuleConfig.Add("DefaultUsername", "USERNAME");
            ModuleConfig.Add("DefaultStatus", "STATUS");
            ModuleConfig.Add("UseStatus", "TRUE");


            return ModuleConfig;
        }

        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void Initialize()
        {
            //module = new AdminModule(new TestUserHandler(), MakeConfig());
            TestUser = new User(identifier, null);
        }

        // Use TestCleanup to run code after each test has run
        [TestCleanup()]
        public void Cleanup()
        {
           // module = new AdminModule(new TestUserHandler() , MakeConfig());
        }

    }
}
