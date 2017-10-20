using Discord;
using Discord.WebSocket;

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SteamBotLite
{
    public abstract class DiscordInterface : ApplicationInterface
    {
        private DiscordSocketClient _client;
       // private DiscordClient _client;
        private List<ulong> BroadCastChatrooms;
        private Game StatusName;
        private string Token;

        public DiscordInterface()
        {
            TickThread = TickThreadState.Stopped;
            Token = config["Token"].ToString();
            BroadCastChatrooms = new List<ulong>();

            if (bool.Parse(config["UseWhitelistAsBroadCastChatrooms"].ToString()))
            {
                foreach (string item in Whitelist)
                {
                    BroadCastChatrooms.Add(ulong.Parse(item));
                }
            }
            else
            {
                foreach (string item in JsonConvert.DeserializeObject<List<string>>(config["Whitelist"].ToString()))
                {
                    BroadCastChatrooms.Add(ulong.Parse(item));
                }
            }

            _client = new DiscordSocketClient();
            ConnectionProcess(Token, _client);
            _client.MessageReceived += _client_MessageReceived;
            AnnounceLoginCompleted();
            Console.WriteLine("Connected to discord!");
        }

     
        public override void BroadCastMessage(object sender, string message)
        {
            foreach (ulong chatroom in BroadCastChatrooms)
            {
                try
                {
                    SocketTextChannel Destination = (Discord.WebSocket.SocketTextChannel)_client.GetChannel(chatroom);
                    Destination.SendMessageAsync(message);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
        }

        public async Task ConnectionProcess(string token, DiscordSocketClient Client)
        {
            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();
            Console.WriteLine("BEGUN THE LOGIN?");
            await Client.StartAsync();
            await Task.Delay(-1);
            
        }

        public void DisconnectionProcess(DiscordSocketClient Client)
        {
            Client.LogoutAsync();
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
            return _client.CurrentUser.Username.ToString();
        }

        public override void LeaveChatroom(object sender, ChatroomEntity ChatroomEntity)
        {
            Console.WriteLine("Leave Chatroom");
        }

        public override void Reboot(object sender, EventArgs e)
        {
            DisconnectionProcess(_client);
            ConnectionProcess(Token, _client).RunSynchronously();
        }

        public override void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus)
        {
            throw new NotImplementedException();
        }

        public override async void SendChatRoomMessageAsync(object sender, MessageEventArgs messagedata)
        {
            try
            {
                SocketTextChannel channel = (SocketTextChannel)messagedata.Chatroom.identifier;

                await channel.SendMessageAsync(messagedata.ReplyMessage);
                await _client.StartAsync();
                Console.WriteLine("Sent Message");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public async Task SendLargeMessageAsync(SocketUser user, string message)
        {
            while (message.Length > 1999)
            {
                await user.SendMessageAsync(message.Substring(0, 1999));
                message = message.Remove(0, 1999);
            }
            await user.SendMessageAsync(message);
            await _client.StartAsync();
            Console.WriteLine("Sent Message");
        }

        public override void SendPrivateMessage(object sender, MessageEventArgs messagedata)
        {
            try
            {
                
                SocketUser user = (SocketUser)messagedata.Destination.ExtraData;

                Console.WriteLine("Casted Fine To Discord");
                SendLargeMessageAsync(user, messagedata.ReplyMessage);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public override void SetStatusMessage(object sender, string message)
        {
            _client.SetGameAsync(message);
        }

        public override void SetUsername(object sender, string Username)
        {
            /*
            _client.CurrentUser.Username = Username;
            _client.CurrentUser.Edit(username: Username);
        */
        }

        public override void tick()
        {
        }

        private System.Threading.Tasks.Task _client_MessageReceived(SocketMessage message)
        {
            Console.WriteLine(message.Content);
            
            SteamBotLite.User user = new SteamBotLite.User(message.Author.Id, this);
            user.DisplayName = message.Author.Username;
            user.ExtraData = message.Author;

            try
            {
                SocketGuildUser MessageSender = (SocketGuildUser)message.Author;
                if (MessageSender.GuildPermissions.KickMembers == true)
                {
                    user.Rank = ChatroomEntity.AdminStatus.True;
                } else
                {
                    user.Rank = ChatroomEntity.AdminStatus.False;
                }
                /*if (message.Author == "admin")
                {
                    user.Rank = ChatroomEntity.AdminStatus.True;
                }
                else
                {
                    user.Rank = ChatroomEntity.AdminStatus.False;
                }*/
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            MessageEventArgs Msg = new MessageEventArgs(this);
            Msg.ReceivedMessage = message.Content;
            Msg.Sender = user;

            Msg.Chatroom = new Chatroom(message.Channel, this);

            if (message.Channel != null)
            {
                ChatRoomMessageProcessEvent(Msg);
            }
            else
            {
                PrivateMessageProcessEvent(Msg);
            }

            return null;
        }
    }
}