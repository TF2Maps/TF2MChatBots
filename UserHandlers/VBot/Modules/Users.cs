using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SteamKit2;
using Newtonsoft.Json;

namespace SteamBotLite
{
    class UsersModule : BaseModule
    {
        public List<object> admins = new List<object>();
        public List<object> bans = new List<object>();

        public UsersModule(VBot bot, Dictionary<string, object> config) : base(bot, config)
        {
            DeletableModule = false;
            loadPersistentData();
        }

        public override string getPersistentData()
        {
            Dictionary<string, List<object>> data = new Dictionary<string, List<object>>();
            data.Add("admins", admins);
            data.Add("bans", bans);
            return JsonConvert.SerializeObject(data);
        }

        public override void loadPersistentData()
        {
            try
            {
                Dictionary<string, List<object>> data;
                data = JsonConvert.DeserializeObject<Dictionary<string, List<object>>>(System.IO.File.ReadAllText(this.GetType().Name + ".json"));
                admins = data["admins"];
                bans = data["bans"];
            }
            catch { }
        }

        public void updateUserInfo(UserIdentifier info, bool IsAdmin)
        {
            if (IsAdmin)
            {
                Console.WriteLine("Admin entered");
                if (!admins.Any(s => info.Equals(s))) //if an admin is not in the list
                {
                    admins.Add(info.identifier);
                    savePersistentData();
                }
            }
            else if (admins.Any(s => info.Equals(s))) //if it's not an admin but he's in the list
            {
                admins.Remove(info.identifier);
                savePersistentData();
            }
        }

        public bool admincheck(UserIdentifier UserToVerify)
        {
            if (UserToVerify.UserRank == UserIdentifier.UserAdminStatus.True | (admins.Any(s => UserToVerify.identifier.ToString().Equals(s))))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
