using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace SteamBotLite
{
    internal partial class MotdModule : BaseModule
    {
        private BaseTask motdPost;
        private MOTDITEM StatusMessageHolder;

        public MotdModule(VBot bot, Dictionary<string, Dictionary<string, object>> Jsconfig) : base(bot, Jsconfig)
        {
            // loading config
            int updateInterval = int.Parse(config["updateInterval"].ToString());

            // loading saved data
            loadPersistentData();

            // loading commands
            commands.Add(new GetMOTD(bot, this));
            commands.Add(new CurrentMotdTick(bot, this));
            commands.Add(new GetMotdSetter(bot, this));
            adminCommands.Add(new SetMotd(bot, this));
            adminCommands.Add(new RemoveMotd(bot, this));
            adminCommands.Add(new SetExtendedMotd(bot, this));
            adminCommands.Add(new EmulateMotd(bot, this));

            motdPost = new BaseTask(updateInterval, new System.Timers.ElapsedEventHandler(MotdPost));
        }

        public string message { get; set; }
        public int postCount { get; set; }
        public int postCountLimit { get; set; }
        public ChatroomEntity setter { get; private set; }
        public string StatusMessage { get; private set; }

        public virtual int DefaultPostCountLimit()
        {
            return 24;
        }

        public virtual string GetName()
        {
            return "MOTD";
        }

        public override string getPersistentData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("motd", message);
            data.Add("count", postCount.ToString());
            data.Add("PostLimit", postCountLimit.ToString());
            return JsonConvert.SerializeObject(data);
        }

        public override void loadPersistentData()
        {
            try
            {
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(System.IO.File.ReadAllText(this.GetType().Name + ".json"));
                message = data["motd"];
                postCount = int.Parse(data["count"]);
                postCountLimit = int.Parse(data["PostLimit"]);
            }
            catch
            {
                postCount = 0;
                postCountLimit = DefaultPostCountLimit();
                motdPost = null;
            }
        }

        /// <summary>
        /// Posts the MOTD to the group chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public virtual void MotdPost(object sender, EventArgs e)
        {
            if (message != null && !message.Equals(string.Empty))
            {
                //bot.SteamFriends.SetPersonaName("MOTD");
                userhandler.BroadcastMessageProcessEvent(message);

                //bot.SteamFriends.SetPersonaName(bot.DisplayName);
                postCount++;
                if (postCount > postCountLimit)
                    message = null;
                savePersistentData();
            }
        }

        public override void OnAllModulesLoaded()
        {
        }

        /// <summary>
        /// Posts the MOTD to the group chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CountDownPost(object sender, EventArgs e)
        {
            if (StatusMessage != null && !StatusMessage.Equals(string.Empty))
            {
                userhandler.SetStatusmessageEvent(message);

                postCount++;
                if (postCount > postCountLimit)
                    message = null;
                savePersistentData();
            }
        }

        /// <summary>
        /// Posts the MOTD to the group chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusMessagePost(object sender, EventArgs e)
        {
            if (message != null && !message.Equals(string.Empty))
            {
                MOTDITEM Parent = (MOTDITEM)sender;

                userhandler.BroadcastMessageProcessEvent(message);
                Parent.postCount++;

                if (Parent.postCount > Parent.postCountLimit)
                {
                    Parent.message = null;
                }

                savePersistentData();
            }
        }

        /// <summary>
        /// Posts the MOTD to the group chat
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StatusPost(object sender, EventArgs e)
        {
            if (message != null && !message.Equals(string.Empty))
            {
                userhandler.SetStatusmessageEvent(message);
                //bot.SteamFriends.SetPersonaName(bot.DisplayName);
                postCount++;
                if (postCount > postCountLimit)
                    message = null;
                savePersistentData();
            }
        }
    }
}