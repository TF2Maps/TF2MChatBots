using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamBotLite;

namespace SteamBotLite
{
    [TestClass]
    public class UnitTest1
    {

        [TestMethod]
        public void TestMethod1()
        {
            ServerModule serverModule = new ServerModule(this, jsconfig);
        }
    }
}
