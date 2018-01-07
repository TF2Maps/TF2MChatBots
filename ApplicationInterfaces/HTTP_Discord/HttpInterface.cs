using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SteamBotLite.ApplicationInterfaces.HTTP_Discord
{
    public class HttpInterface : ApplicationInterface
    {
        string LastMsg = "";
        Dictionary<string, string> Last_Message;
        string Username = "VBot-C#";
        List<string> Admins_Roles;

        public HttpInterface()
        {
           // this.WebHook = config["WebHook"].ToString();
            this.token = config["Token"].ToString();
            this.Private_Msgs_Webhook = config["PrivateMsgs"].ToString();
            this.Broadcasts_Webhook = config["Broadcasts"].ToString();
            Admins_Roles = JsonConvert.DeserializeObject<List<string>>(config["Admins"].ToString());
            channels = JsonConvert.DeserializeObject<Dictionary<string,string>>(config["Channels"].ToString());
            Last_Message = new Dictionary<string, string>();

            foreach (KeyValuePair<string,string> chatroom in channels)
            {
                GetLatestMessages(chatroom.Key);
            }

            GetChannelMessages(channels);
            SendMessageThroughWebhook("Hello world", config["WebHook"].ToString());
        }

        string token = "";
        //string WebHook = "";
        string Private_Msgs_Webhook = "";
        string Broadcasts_Webhook = "";

        Dictionary<string, string> channels;
        public override void BroadCastMessage(object sender, string message)
        {
            SendMessageThroughWebhook(message, Broadcasts_Webhook);
        }
        public override void SendChatRoomMessage(object sender, MessageEventArgs messagedata)
        {
            SendMessageThroughWebhook(messagedata.ReplyMessage, channels[messagedata.Chatroom.identifier.ToString()]);
        }

        public override void SendPrivateMessage(object sender, MessageEventArgs messagedata)
        {
            string prevusername = Username;
            Username = prevusername + "-Private";
            messagedata.ReplyMessage = "<@" + messagedata.Destination.identifier + "> " + "\n" + messagedata.ReplyMessage;


            SendMessageThroughWebhook(messagedata.ReplyMessage, Private_Msgs_Webhook);
            Username = prevusername;
        }


        public override void EnterChatRoom(object sender, ChatroomEntity ChatroomEntity)
        {}

        public override string GetOthersUsername(object sender, ChatroomEntity user) { return "N/A"; }

        public override string GetUsername() { return "N/A"; }

        public override void LeaveChatroom(object sender, ChatroomEntity ChatroomEntity)
        {}

        public override void Reboot(object sender, EventArgs e)
        {}

        public override void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus)
        {}

        public override void SetStatusMessage(object sender, string message)
        {}

        public override void SetUsername(object sender, string Username)
        {}

        public override void tick()
        {
            try
            {
                var recent_msgs = GetChannelMessages(channels);
                foreach (var msg in recent_msgs)
                {
                    if (msg.Sender.Rank == ChatroomEntity.AdminStatus.True) {
                        ChatRoomMessageProcessEvent(msg);
                    } else {
                        ChatRoomMessageProcessEvent(msg);
                    }
                }
            }
            catch (Exception e)
            {

            }
        }

        void GetLatestMessages(string chatroom)
        {
            try
            {


                var client = new RestClient("https://discordapp.com/api/v6/channels/" + chatroom + "/messages");
                var request = new RestRequest(Method.GET);
                request.AddHeader("postman-token", "ad8e82e8-c4bc-33b2-b891-6ab7d35a0fda");
                request.AddHeader("cache-control", "no-cache");
                request.AddHeader("authorization", "Bot " + token);
                IRestResponse response = client.Execute(request);

                List<MessageEventArgs> Returning = new List<MessageEventArgs>();

                List<ChannelMessages> ChannelMessages = JsonConvert.DeserializeObject<List<ChannelMessages>>(response.Content);

                if (ChannelMessages.Count > 0)
                {
                    Last_Message[chatroom] = ChannelMessages[0].id;
                }
            }
            catch
            {

            }
        }

        void SendMessageThroughWebhook(string message, string URL)
        {
            System.Threading.Thread.Sleep(500);

            string[] AllWords = message.Split(' ');
            string newreply = "";

            foreach (string word in AllWords) {
                string append = word + " ";
                if (append.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    append = "<" + append.Trim(' ') + ">";
                }
                newreply += append;
            }
            message = newreply;


            string full_message = message;
            if (full_message.Length > 1900)
            {
                message = message.Substring(0, 1900);
            }

            var client = new RestClient(URL);
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "2e66de8f-1f2c-d2f9-97cb-fc3b69c1d071");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "content=" + WebUtility.UrlEncode(message) + "&username=" + WebUtility.UrlEncode(Username), ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            if (full_message.Length > 1900)
            {
                SendMessageThroughWebhook(message.Substring(1900, message.Length - 1900), URL);
            }
        }

        public class Author
        {
            public string username { get; set; }
            public string discriminator { get; set; }
            public string id { get; set; }
            public string avatar { get; set; }
        }

        public class ChannelMessages
        {
            public List<object> attachments { get; set; }
            public bool tts { get; set; }
            public List<object> embeds { get; set; }
            public DateTime timestamp { get; set; }
            public bool mention_everyone { get; set; }
            public string id { get; set; }
            public bool pinned { get; set; }
            public object edited_timestamp { get; set; }
            public Author author { get; set; }
            public List<object> mention_roles { get; set; }
            public string content { get; set; }
            public string channel_id { get; set; }
            public List<object> mentions { get; set; }
            public int type { get; set; }
        }
        public List<MessageEventArgs> GetChannelMessages(Dictionary<string,string> all_channels)
        {
            List<MessageEventArgs> responses = new List<MessageEventArgs>() ;
            try {
                foreach (KeyValuePair<string,string> channel in all_channels)
                {
                    foreach (var response in GetChannelMessages(channel.Key))
                    {
                        responses.Add(response);
                    }
                }
            }
            catch { }
            return responses;

        }

        public class GuildPermissionsReturn
        {
            public bool deaf { get; set; }
            public DateTime joined_at { get; set; }
            public User user { get; set; }
            public List<string> roles { get; set; }
            public bool mute { get; set; }
        }

        public bool CheckUserIsAdmin (string author) {
            var client = new RestClient("https://discordapp.com/api/v6/guilds/217585440457228290/members/" + author);
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "4906efa5-8a48-bee3-2e58-37701c20c21b");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bot " + token);
            IRestResponse response = client.Execute(request);

            GuildPermissionsReturn Perms = JsonConvert.DeserializeObject<GuildPermissionsReturn>(response.Content);
            foreach (string role in Perms.roles) {
                if (Admins_Roles.Contains(role)) {
                    return true;
                }
            }
            return false;
        }

        public List<MessageEventArgs> GetChannelMessages(string channel)
        {
            
            System.Threading.Thread.Sleep(125);
            var client = new RestClient("https://discordapp.com/api/v6/channels/" + channel + "/messages?after=" + Last_Message[channel]);
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "ad8e82e8-c4bc-33b2-b891-6ab7d35a0fda");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bot " + token);
            IRestResponse response = client.Execute(request);

            List<MessageEventArgs> Returning = new List<MessageEventArgs>();

            List<ChannelMessages> ChannelMessages = JsonConvert.DeserializeObject<List<ChannelMessages>>(response.Content);

            if (ChannelMessages.Count > 0)
            {
                Last_Message[channel] = ChannelMessages[0].id;
            }
            

            foreach (ChannelMessages msg in ChannelMessages)
            {
                MessageEventArgs Converted_msg = new MessageEventArgs(this);
                Converted_msg.Chatroom = new ChatroomEntity(channel, this);
                Converted_msg.ReceivedMessage = msg.content;
                var sender = new ChatroomEntity(msg.author.id, this);
                sender.DisplayName = msg.author.username;
                sender.ParentIdentifier = channel;
                Converted_msg.Sender = sender;
                if (CheckUserIsAdmin(msg.author.id)){
                    sender.Rank = ChatroomEntity.AdminStatus.True;
                }
                
                Returning.Add(Converted_msg);
            }
            return Returning;
        }

    }
}
