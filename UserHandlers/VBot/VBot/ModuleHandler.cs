using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SteamBotLite
{
    public interface ModuleHandler
    {
        void OnMaplistchange(ObservableCollection<MapModule.Map> mapList, object sender, NotifyCollectionChangedEventArgs args);
        void HTMLFileFromArray(string[] headerNames, List<string[]> dataEntries, string v);
        void SendPrivateMessageProcessEvent(MessageEventArgs messageEventArgs);
        void SetStatusmessageEvent(string statusMessage);
        void BroadcastMessageProcessEvent(string message);
        bool admincheck(ChatroomEntity user);
    }
}