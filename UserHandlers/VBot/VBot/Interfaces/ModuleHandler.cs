using System.Collections.Generic;
using System.Collections.Specialized;

namespace SteamBotLite
{
    public interface ModuleHandler 
    {
        void AddListChangeEventListiner(MapListChangeListiner listiner);

        void AddLoginEventListiner(OnLoginCompletedListiners listiner);

        void AddMapChangeEventListiner(ServerMapChangeListiner listiner);

        void AddModuleToCurrentModules(BaseModule module);

        bool admincheck(ChatroomEntity user);

        void BroadcastMessageProcessEvent(string message);

        List<BaseModule> GetAllModules();

     

        void OnMaplistchange(IReadOnlyList<Map> mapList, object sender, NotifyCollectionChangedEventArgs args);

        void SendPrivateMessageProcessEvent(MessageEventArgs messageEventArgs);

        void ServerUpdated(object sender, TrackingServerInfo args);

        void SetStatusmessageEvent(string statusMessage);

        void UpdateUsernameEvent(object sender, string Username);
    }
}