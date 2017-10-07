namespace SteamBotLite
{
    public interface ServerMapChangeListiner
    {
        void OnMapChange(TrackingServerInfo args);
    }
}