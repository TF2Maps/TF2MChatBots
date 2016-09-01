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
        public List<string> admins = new List<string>();
        public List<string> bans = new List<string>();

        public UsersModule(VBot bot, Dictionary<string, object> config) : base(bot, config)
        {
            DeletableModule = false;
            loadPersistentData();
        }

        public override string getPersistentData()
        {
            Dictionary<string, List<string>> data = new Dictionary<string, List<string>>();
            data.Add("admins", admins);
            data.Add("bans", bans);
            return JsonConvert.SerializeObject(data);
        }

        public override void loadPersistentData()
        {
            try
            {
                Dictionary<string, List<string>> data; 
                data = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(this.GetType().Name + ".json"));
                admins = data["admins"];
                bans = data["bans"];
            }
            catch { }
        }

        public void updateUserInfo(SteamFriends.ChatMemberInfoCallback info)
        {
            
            string user = info.StateChangeInfo.ChatterActedOn.ToString();
            EChatPermission status = info.StateChangeInfo.MemberInfo.Permissions;
            Console.WriteLine("User {0} entred with status: {1}", info.StateChangeInfo.ChatterActedOn,info.StateChangeInfo.MemberInfo.Permissions);
            
            if (status.HasFlag(EChatPermission.MemberDefault))
            {
                Console.WriteLine("Admin entered");
                if (!admins.Any(s => user.Equals(s))) //if an admin is not in the list
                {
                    admins.Add(user);
                    savePersistentData();
                }
            }
            else if (admins.Any(s => user.Equals(s))) //if it's not an admin but he's in the list
            {
                admins.Remove(user);
                savePersistentData();
            }
               

        }
        public bool admincheck(SteamID UserToVerify)
        {
            return admins.Any(s => UserToVerify.ToString().Equals(s));
        }
    }
}
