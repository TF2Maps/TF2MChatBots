using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace SteamBotLite
{
    internal class TestUserHandler : UserHandler, ModuleHandler, IHTMLFileFromArrayListiners
    {
        public void AddWebsiteEntry(string identifier, TableDataValue[] data, int limit)
        {
        }

        public void AddWebsiteEntryWithoutLimit(string identifier, TableDataValue[] data)
        {
        }

        public void AddHTMLTable(string TableKey, string Tabledata)
        {
        }

        void ModuleHandler.AddListChangeEventListiner(MapListChangeListiner listiner)
        {
        }

        void ModuleHandler.AddLoginEventListiner(OnLoginCompletedListiners listiner)
        {
        }

        void ModuleHandler.AddMapChangeEventListiner(ServerMapChangeListiner listiner)
        {
        }

        bool ModuleHandler.admincheck(ChatroomEntity user)
        {
            if (user.Rank == ChatroomEntity.AdminStatus.True)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        void ModuleHandler.BroadcastMessageProcessEvent(string message)
        {
        }

        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e)
        {
        }

        List<BaseModule> ModuleHandler.GetAllModules()
        {
            return new List<BaseModule>();
        }

        void IHTMLFileFromArrayListiners.HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey)
        {
        }

        void ModuleHandler.HTMLFileFromArray(string[] headerNames, List<string[]> dataEntries, string v)
        {
        }

        public void MakeTableFromEntry(string TableKey, TableData TableData)
        {
        }

        public override void OnLoginCompleted(object sender, EventArgs e)
        {
        }

        void ModuleHandler.OnMaplistchange(IReadOnlyList<Map> mapList, object sender, NotifyCollectionChangedEventArgs args)
        {
        }

        public override void ProcessChatRoomMessage(object sender, MessageEventArgs e)
        {
        }

        public override void ProcessPrivateMessage(object sender, MessageEventArgs e)
        {
        }

        void ModuleHandler.SendPrivateMessageProcessEvent(MessageEventArgs messageEventArgs)
        {
        }

        void ModuleHandler.ServerUpdated(object sender, TrackingServerInfo args)
        {
        }

        void ModuleHandler.SetStatusmessageEvent(string statusMessage)
        {
        }

        public void SetTableHeader(string TableIdentifier, TableDataValue[] Header)
        {
        }

        void ModuleHandler.UpdateUsernameEvent(object sender, string Username)
        {
        }

        public void AddModuleToCurrentModules(BaseModule module)
        {
        }
    }
}