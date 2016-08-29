using Newtonsoft.Json;
using SteamKit2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite
{
    class MotdModule : BaseModule
    {
        public int postCount {get; private set;}
        public string message {get; private set;}
        public SteamID setter {get; private set;}

        private BaseTask motdPost;

        public MotdModule(VBot bot, Dictionary<string, object> Jsconfig) : base(bot, Jsconfig)
        {
            // loading config 
            int updateInterval = int.Parse(config["updateInterval"].ToString());

            // loading saved data
            loadPersistentData();

            // loading commands
            commands.Add(new Get(bot, this));
            commands.Add(new Tick(bot, this));
            commands.Add(new Setter(bot, this));
            adminCommands.Add(new Set(bot, this));
            adminCommands.Add(new Remove(bot, this));

            motdPost = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(MotdPost));
        }

        public override string getPersistentData()
        {

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("motd", message);
            data.Add("count", postCount.ToString());

            return JsonConvert.SerializeObject(data);
        }

        public override void loadPersistentData()
        {
            try
            {
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(this.GetType().Name + ".json"));
                message = data["motd"];
                postCount = int.Parse(data["count"]);
            }
            catch
            {
                postCount = 0;
                motdPost = null;
            }
        }

        /// <summary>
        /// Posts the MOTD to the group chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MotdPost(object sender, EventArgs e)
        {
            if (message != null && !message.Equals(string.Empty))
            {
                string currentName = userhandler.steamConnectionHandler.SteamFriends.GetPersonaName();
                //bot.SteamFriends.SetPersonaName("MOTD");
                userhandler.steamConnectionHandler.SteamFriends.SendChatRoomMessage(userhandler.GroupChatSID, EChatEntryType.ChatMsg, message);
                //bot.SteamFriends.SetPersonaName(bot.DisplayName);
                postCount++;
                if (postCount > 24)
                    message = null;
                savePersistentData();
            }
        }

        // The abstract command for motd

        abstract public class MotdCommand : BaseCommand
        {
            protected MotdModule motd;

            public MotdCommand(VBot bot, string command, MotdModule motd) : base(bot, command)
            {
                this.motd = motd;
            }
        }

        // The commands

        private class Get : MotdCommand
        {
            public Get(VBot bot, MotdModule motd) : base(bot, "!Motd", motd) { }
            protected override string exec(SteamID sender, string param)
            {
                if (motd.postCount >= 24){
                    motd.message = null;
                    motd.postCount = 0;
                }
                if (motd.message != null)
                    motd.postCount++;

                return motd.message;
            }
        }

        private class Set : MotdCommand
        {
            public Set(VBot bot, MotdModule motd) : base(bot, "!SetMotd", motd) { }
            protected override string exec(SteamID sender, string param)
            {
                if (param == String.Empty)
                    return "Make sure to include a MOTD to display!";
                    
                if (motd.message != null)
                    return "There is currently a MOTD, please remove it first";

                motd.message = param;
                motd.setter = sender;
                motd.postCount = 0;
                motd.savePersistentData();
                return "MOTD Set to: " + motd.message;
            }
        }

        private class Remove : MotdCommand
        {
            public Remove(VBot bot, MotdModule motd) : base(bot, "!RemoveMotd", motd) { }
            protected override string exec(SteamID sender, string param)
            {
                motd.message = null;
                motd.setter = null;
                motd.postCount = 0;
                motd.savePersistentData();
                return "Removed MOTD";
            }
        }

        private class Tick : MotdCommand
        {
            public Tick(VBot bot, MotdModule motd) : base(bot, "!MotdTick", motd) { }
            protected override string exec(SteamID sender, string param)
            {
                return "MOTD displayed " + motd.postCount + " times";
            }
        }

        private class Setter : MotdCommand
        {
            public Setter(VBot bot, MotdModule motd) : base(bot, "!MotdSetter", motd) { }
            protected override string exec(SteamID sender, string param)
            {
                return "MOTD set by " + motd.setter;
            }
        }
    }
}
