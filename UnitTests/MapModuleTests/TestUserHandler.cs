using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    class TestUserHandler : UserHandler, ModuleHandler
    {
        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e)
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

        bool ModuleHandler.admincheck(ChatroomEntity user)
        {
            return false;
        }

        void ModuleHandler.BroadcastMessageProcessEvent(string message)
        {
        }

        void ModuleHandler.HTMLFileFromArray(string[] headerNames, List<string[]> dataEntries, string v)
        {
        }

        void ModuleHandler.OnMaplistchange(ObservableCollection<MapModule.Map> mapList, object sender, NotifyCollectionChangedEventArgs args)
        {
        }

        void ModuleHandler.SendPrivateMessageProcessEvent(MessageEventArgs messageEventArgs)
        {
        }

        void ModuleHandler.SetStatusmessageEvent(string statusMessage)
        {
        }
    }
}
