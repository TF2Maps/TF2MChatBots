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
        string Username = "VBot-C#";

        public HttpInterface()
        {
            this.WebHook = config["WebHook"].ToString();
            this.token = config["Token"].ToString();
            GetLatestMessages();
            GetChannelMessages();
            SendMessageThroughWebhook("Hello world", config["WebHook"].ToString() );
        }
        string token = "";
        string WebHook = "";
        public override void BroadCastMessage(object sender, string message)
        {
            SendMessageThroughWebhook(message, WebHook);
        }
        public override void SendChatRoomMessage(object sender, MessageEventArgs messagedata)
        {
            SendMessageThroughWebhook(messagedata.ReplyMessage, WebHook);
        }

        public override void SendPrivateMessage(object sender, MessageEventArgs messagedata)
        {
            string prevusername = Username;
            Username = prevusername + "-Private";
            SendMessageThroughWebhook(messagedata.ReplyMessage, WebHook);
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
                var recent_msgs = GetChannelMessages();
                foreach (var msg in recent_msgs)
                {
                    ChatRoomMessageProcessEvent(msg);
                }
            }
            catch (Exception e)
            {

            }
        }

        void GetLatestMessages()
        {
            var client = new RestClient("https://discordapp.com/api/v6/channels/346831363787456513/messages");
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "ad8e82e8-c4bc-33b2-b891-6ab7d35a0fda");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bot " + token);
            IRestResponse response = client.Execute(request);

            List<MessageEventArgs> Returning = new List<MessageEventArgs>();

            List<ChannelMessages> ChannelMessages = JsonConvert.DeserializeObject<List<ChannelMessages>>(response.Content);

            if (ChannelMessages.Count > 0)
            {
                LastMsg = ChannelMessages[0].id;
            }

        }
        void SendMessageThroughWebhook(string message, string URL)
        {
            var client = new RestClient(URL);
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "2e66de8f-1f2c-d2f9-97cb-fc3b69c1d071");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("content-type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", "content=" + WebUtility.UrlEncode(message) + "&username=" + WebUtility.UrlEncode(Username) , ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
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

        public List<MessageEventArgs> GetChannelMessages()
        {
            
            System.Threading.Thread.Sleep(1000);
            var client = new RestClient("https://discordapp.com/api/v6/channels/346831363787456513/messages?after=" + LastMsg);
            var request = new RestRequest(Method.GET);
            request.AddHeader("postman-token", "ad8e82e8-c4bc-33b2-b891-6ab7d35a0fda");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("authorization", "Bot " + token);
            IRestResponse response = client.Execute(request);

            List<MessageEventArgs> Returning = new List<MessageEventArgs>();

            List<ChannelMessages> ChannelMessages = JsonConvert.DeserializeObject<List<ChannelMessages>>(response.Content);

            if (ChannelMessages.Count > 0)
            {
                LastMsg = ChannelMessages[0].id;
            }
            

            foreach (ChannelMessages msg in ChannelMessages)
            {
                MessageEventArgs Converted_msg = new MessageEventArgs(this);
                Converted_msg.Chatroom = new ChatroomEntity("346831363787456513", this);
                Converted_msg.ReceivedMessage = msg.content;
                var sender = new ChatroomEntity(msg.author.id, this);
                sender.DisplayName = msg.author.username;
                sender.ParentIdentifier = "346831363787456513";
                Converted_msg.Sender = sender;
                
                Returning.Add(Converted_msg);
            }
            return Returning;
        }

    }
}
