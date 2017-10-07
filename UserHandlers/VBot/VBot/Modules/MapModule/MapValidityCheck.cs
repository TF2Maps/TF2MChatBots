using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{

    public class MapValidityCheck
    {
        public bool IsValid = true;
        public string ReturnMessage;

        public void SetInvalid(string message)
        {
            IsValid = false;
            ReturnMessage = message;
        }
    }

}