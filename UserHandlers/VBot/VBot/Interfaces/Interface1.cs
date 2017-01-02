using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    interface ServerMapChangeListiner
    {
        void OnMapChange(ServerModule.ServerInfo args);
    }
}
