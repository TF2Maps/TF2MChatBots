using Microsoft.VisualStudio.TestTools.UnitTesting;
using SteamBotLite;
using System.Collections.Generic;

namespace UsersModuleTests
{
    [TestClass()]
    public class UsersModuleUsersModuleTest
    {
        private ChatroomEntity Admin;
        private ChatroomEntity AdminDeterminedByConfig;
        private string AdminDeterminedByConfigIdentifier = "4";
        private string AdminIdentifier = "0";
        private UsersModule module;
        private ChatroomEntity NonAdmin;
        private string NonAdminIdentifier = "1";

        private ChatroomEntity OtherUser;
        private string OtherUserIdentifier = "3";
        private ChatroomEntity UnknownUser;
        private string UnknownUserIdentifier = "2";

        [TestMethod()]
        public void AddAdmin()
        {
            module.updateUserInfo(AdminDeterminedByConfig, true);
            Assert.IsTrue(module.admincheck(AdminDeterminedByConfig));
        }

        [TestMethod()]
        public void admincheckUsersModuleTest()
        {
            Assert.IsTrue(module.admincheck(Admin));

            Assert.IsFalse(module.admincheck(NonAdmin));
            Assert.IsFalse(module.admincheck(OtherUser));
            Assert.IsFalse(module.admincheck(UnknownUser));
        }

        [TestCleanup()]
        public void Cleanup()
        {
            module = new UsersModule(new TestUserHandler(), MakeConfig());

            module.admins.Clear();
            module.bans.Clear();

            module.savePersistentData();
            module.loadPersistentData();

            Assert.IsTrue(module.admins.Count == 0);
            Assert.IsTrue(module.bans.Count == 0);
        }

        [TestMethod()]
        public void DontAddSomeoneNotInList()
        {
            module.updateUserInfo(AdminDeterminedByConfig, false);
            Assert.IsFalse(module.admincheck(AdminDeterminedByConfig));
        }

        [TestMethod()]
        public void EnsurePersistencyForAdmin()
        {
            module.updateUserInfo(AdminDeterminedByConfig, true);
            module = new UsersModule(new TestUserHandler(), MakeConfig());

            Assert.IsTrue(module.admincheck(AdminDeterminedByConfig));
        }

        [TestMethod()]
        public void EnsurePersistencyForChangingAdmin()
        {
            module.updateUserInfo(AdminDeterminedByConfig, true);
            module = new UsersModule(new TestUserHandler(), MakeConfig());
            Assert.IsTrue(module.admincheck(AdminDeterminedByConfig));

            module.updateUserInfo(AdminDeterminedByConfig, false);
            Assert.IsFalse(module.admincheck(AdminDeterminedByConfig));

            module = new UsersModule(new TestUserHandler(), MakeConfig());
            Assert.IsFalse(module.admincheck(AdminDeterminedByConfig));
        }

        [TestInitialize()]
        public void Initialize()
        {
            module = new UsersModule(new TestUserHandler(), MakeConfig());

            Admin = new ChatroomEntity(AdminIdentifier, null);
            Admin.Rank = ChatroomEntity.AdminStatus.True;

            NonAdmin = new ChatroomEntity(NonAdminIdentifier, null);
            NonAdmin.Rank = ChatroomEntity.AdminStatus.False;

            UnknownUser = new ChatroomEntity(UnknownUserIdentifier, null);
            UnknownUser.Rank = ChatroomEntity.AdminStatus.Unknown;

            OtherUser = new ChatroomEntity(OtherUserIdentifier, null);
            OtherUser.Rank = ChatroomEntity.AdminStatus.Other;

            AdminDeterminedByConfig = new ChatroomEntity(AdminDeterminedByConfigIdentifier, null);
            AdminDeterminedByConfig.Rank = ChatroomEntity.AdminStatus.False;
        }

        private Dictionary<string, Dictionary<string, object>> MakeConfig()
        {
            Dictionary<string, Dictionary<string, object>> ModuleHolder = new Dictionary<string, Dictionary<string, object>>();

            ModuleHolder.Add("UsersModule", new Dictionary<string, object>());
            return ModuleHolder;
        }
    }
}