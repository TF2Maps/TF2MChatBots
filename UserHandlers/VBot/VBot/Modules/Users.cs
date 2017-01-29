using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SteamKit2;
using Newtonsoft.Json;

namespace SteamBotLite
{
    public class UsersModule : BaseModule
    {
        public List<string> admins = new List<string>();
        public List<string> bans = new List<string>();

        

        public UsersModule(VBot bot, Dictionary<string, Dictionary<string, object>> config) : base(bot, config)
        {
            DeletableModule = false;
            loadPersistentData();
        }

        public override void OnAllModulesLoaded()
        {
           

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
                data = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(System.IO.File.ReadAllText(ModuleSavedDataFilePath()));
                admins = data["admins"];
                bans = data["bans"];
            }
            catch { }
        }

        public void updateUserInfo(ChatroomEntity info, bool IsAdmin)
        {
            if (IsAdmin)
            {
                Console.WriteLine("Admin entered");
                if (!admins.Any(s => info.Equals(s))) //if an admin is not in the list
                {
                    admins.Add(info.identifier.ToString());
                    savePersistentData();
                }
            }
            else if (admins.Any(s => info.Equals(s))) //if it's not an admin but he's in the list
            {
                admins.Remove(info.identifier.ToString());
                savePersistentData();
            }
        }

        public bool admincheck(ChatroomEntity UserToVerify)
        {
            string data = UserToVerify.identifier.ToString();
            if (UserToVerify.Rank == ChatroomEntity.AdminStatus.True | (admins.Any(s => UserToVerify.identifier.ToString().Equals(s))))
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
