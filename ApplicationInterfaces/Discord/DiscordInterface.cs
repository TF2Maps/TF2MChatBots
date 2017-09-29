using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json;

namespace SteamBotLite
{

    public abstract class DiscordInterface : ApplicationInterface
    {
        private DiscordClient _client;
        string Token;

        List<ulong> BroadCastChatrooms;

        public DiscordInterface()
        {
            TickThread = TickThreadState.Stopped;
            Token = config["Token"].ToString();
            BroadCastChatrooms = new List<ulong>();

            if (bool.Parse(config["UseWhitelistAsBroadCastChatrooms"].ToString()))  {
                foreach (string item in Whitelist)  {
                    BroadCastChatrooms.Add(ulong.Parse(item));
                }
            }
            else {
                foreach (string item in JsonConvert.DeserializeObject<List<string>>(config["Whitelist"].ToString()))  {
                    BroadCastChatrooms.Add(ulong.Parse(item));
                }
            }

            _client = new DiscordClient();
            ConnectionProcess(Token, _client);
            _client.MessageReceived += _client_MessageReceived;
        }

        private void _client_MessageReceived(object sender, Discord.MessageEventArgs e)
        {
            Console.WriteLine(e.Message.RawText);
            if (!e.Message.IsAuthor)
            {
                SteamBotLite.User user = new SteamBotLite.User(e.User.Id, this);
                user.DisplayName = e.User.Name;
                user.ExtraData = e.User;
                
               
                if (e.User.ServerPermissions.Administrator)
                {
                    user.Rank = ChatroomEntity.AdminStatus.True;
                }
                else
                {
                    user.Rank = ChatroomEntity.AdminStatus.False;
                }

                MessageEventArgs Msg = new MessageEventArgs(this);
                Msg.ReceivedMessage = e.Message.RawText;
                Msg.Sender = user;

                Msg.Chatroom = new Chatroom(e.Message.Channel,  this);
                
                if (e.Message.Channel != null)
                {
                    ChatRoomMessageProcessEvent(Msg);
                }
                else
                {
                    PrivateMessageProcessEvent(Msg);
                }
            }
        }

        public void ConnectionProcess(string token, DiscordClient Client)
        {
            Client.Connect(token, TokenType.Bot);            
        }

        public void DisconnectionProcess(DiscordClient Client)
        {
            _client.ExecuteAndWait(async () => {
                await Client.Disconnect();
            });
        }


        public override void EnterChatRoom(object sender, ChatroomEntity ChatroomEntity)
        {
            Console.WriteLine("Enter Chatroom");
        }

        public override string GetOthersUsername(object sender, ChatroomEntity user)
        {
            return user.DisplayName;
        }

        public override string GetUsername()
        {
            return _client.CurrentUser.Name.ToString();
        }

        public override void LeaveChatroom(object sender, ChatroomEntity ChatroomEntity)
        {
            Console.WriteLine("Leave Chatroom");
        }

        public override void Reboot(object sender, EventArgs e)
        {
            DisconnectionProcess(_client);
            ConnectionProcess(Token, _client);
        }

        public override void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus)
        {
            throw new NotImplementedException();
        }

        public override void SendChatRoomMessage(object sender, MessageEventArgs messagedata)
        {
            try
            {
                Channel channel = (Channel)messagedata.Chatroom.identifier;
                channel.SendMessage(messagedata.ReplyMessage);
            }
            catch
            {

            }
        }

        public override void SendPrivateMessage(object sender, MessageEventArgs messagedata)
        {
            try
            {
                Discord.User user = (Discord.User)messagedata.Sender.ExtraData;
                
                Console.WriteLine("Casted Fine To Discord");
                SendLargeMessage(user, messagedata.ReplyMessage);
            }
            catch
            {
                Console.WriteLine("Casting Error");
            }
        }
         
        public void SendLargeMessage (Discord.User user , string message)
        {
            while (message.Length > 1999)
            {
                user.SendMessage(message.Substring(0, 1999));
                message = message.Remove(0, 1999);

            }
            user.SendMessage(message);
        }
        

        public override void SetUsername(object sender, string Username)
        {
            _client.CurrentUser.Edit(username: Username);
           
        }
        
        public override void tick()
        {
            
        }

        public override void BroadCastMessage(object sender, string message)
        {
            foreach (ulong chatroom in BroadCastChatrooms)
            {
                try
                {
                    Channel Destination = _client.GetChannel(chatroom);
                    Destination.SendMessage(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }
        }

        Game StatusName;
        public override void SetStatusMessage(object sender, string message)
        {
            StatusName = new Game(message);
            _client.SetGame(StatusName);
        }
    }
}
