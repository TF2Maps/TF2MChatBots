using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    class TestUserHandler : UserHandler, ModuleHandler, HTMLFileFromArrayListiners
    {
        public void AddEntryWithLimit(string identifier, TableDataValue[] data, int limit)
        {
        }

        public void AddEntryWithoutLimit(string identifier, TableDataValue[] data)
        {
        }

        public void AddHTMLTable(string TableKey, string Tabledata)
        {
        }

        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e)
        {
        }

        public void MakeTableFromEntry(string TableKey, TableData TableData)
        {
        }

        public override void OnLoginCompleted(object sender, EventArgs e)
        {
        }

        public override void ProcessChatRoomMessage(object sender, MessageEventArgs e)
        {
        }

        public override void ProcessPrivateMessage(object sender, MessageEventArgs e)
        {
        }

        public void SetTableHeader(string TableIdentifier, TableDataValue[] Header)
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
            if (user.Rank == ChatroomEntity.AdminStatus.True) {
                return true;
            }
            else {
                return false;
            }
        }

        void ModuleHandler.BroadcastMessageProcessEvent(string message)
        {
        }

        void ModuleHandler.Disablemodule(string param) {
        }

        void ModuleHandler.Enablemodule(string param)
        {
        }

        List<BaseModule> ModuleHandler.GetAllModules() {
            return new List<BaseModule>();
        }

        void HTMLFileFromArrayListiners.HTMLFileFromArray(string[] Headernames, List<string[]> Data, string TableKey)
        {
        }

        void ModuleHandler.HTMLFileFromArray(string[] headerNames, List<string[]> dataEntries, string v)
        {
        }

        void ModuleHandler.OnMaplistchange(IReadOnlyList<Map> mapList, object sender, NotifyCollectionChangedEventArgs args)
        {
        }

        void ModuleHandler.SendPrivateMessageProcessEvent(MessageEventArgs messageEventArgs)
        {
        }

        void ModuleHandler.ServerUpdated(object sender, ServerInfo args)
        {
        }

        void ModuleHandler.SetStatusmessageEvent(string statusMessage)
        {
        }


        void ModuleHandler.UpdateUsernameEvent(object sender, string Username)
        {
        }
    }
}
