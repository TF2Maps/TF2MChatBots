using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SteamBotLite
{
    public interface ModuleHandler
    {
        void OnMaplistchange(IReadOnlyList<Map> mapList, object sender, NotifyCollectionChangedEventArgs args);
        void HTMLFileFromArray(string[] headerNames, List<string[]> dataEntries, string v);
       
        void SendPrivateMessageProcessEvent(MessageEventArgs messageEventArgs);
        void SetStatusmessageEvent( string statusMessage);
        void UpdateUsernameEvent(object sender , string Username);
        void BroadcastMessageProcessEvent(string message);
        bool admincheck(ChatroomEntity user);
        void AddMapChangeEventListiner(ServerMapChangeListiner listiner);
        void ServerUpdated(object sender, ServerInfo args);

        void AddLoginEventListiner(OnLoginCompletedListiners listiner);
        void AddListChangeEventListiner(MapListChangeListiner listiner);
        List<BaseModule> GetAllModules();
    }
}