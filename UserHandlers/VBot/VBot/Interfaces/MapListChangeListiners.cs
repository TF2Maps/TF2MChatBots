using System.Collections.Generic;

namespace SteamBotLite
{
    public interface MapListChangeListiner
    {
        void MaplistChange(IReadOnlyList<Map> maplist);
    }
}