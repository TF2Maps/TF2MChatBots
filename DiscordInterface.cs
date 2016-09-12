using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SteamBotLite
{

    public class DiscordInterface : ApplicationInterface
    {
        private DiscordClient _client;
        string Token = "MjIyMjA0MDQ2MjYwMzA1OTIy.CrQ1MA.StYrm9OA2qsJxWv9kcD0_GvwBlU";

        public DiscordInterface(string token , ulong DiscordChatID) : base(DiscordChatID)
        {
            _client = new DiscordClient();
            ConnectionProcess(token, _client);
            _client.MessageReceived += _client_MessageReceived;
        }

        private void _client_MessageReceived(object sender, MessageEventArgs e)
        {
            Console.WriteLine(e.Message.RawText);
            if (!e.Message.IsAuthor)
            {

                UserIdentifier User = new UserIdentifier(e.User.Id);
                User.DisplayName = e.User.Name;
                if (e.User.ServerPermissions.Administrator)
                {
                    User.UserRank = UserIdentifier.UserAdminStatus.True;
                }
                else
                {
                    User.UserRank = UserIdentifier.UserAdminStatus.False;
                }

                MessageProcessEventData Msg = new MessageProcessEventData(this);
                Msg.ReceivedMessage = e.Message.RawText;
                Msg.Sender = User;
                Msg.Chatroom = new ChatRoomIdentifier(e.Message.Channel);
               
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


        public override void EnterChatRoom(object sender, ChatRoomIdentifier chatroomidentifier)
        {
            Console.WriteLine("Enter Chatroom");
        }

        public override string GetOthersUsername(object sender, UserIdentifier user)
        {
            return user.DisplayName;
        }

        public override string GetUsername()
        {
            return _client.CurrentUser.Name;
        }

        public override void LeaveChatroom(object sender, ChatRoomIdentifier chatroomidentifier)
        {
            Console.WriteLine("Leave Chatroom");
        }

        public override void Reboot(object sender, EventArgs e)
        {
            DisconnectionProcess(_client);
            ConnectionProcess(Token, _client);
        }

        public override void ReceiveChatMemberInfo(UserIdentifier useridentifier, bool AdminStatus)
        {
            throw new NotImplementedException();
        }

        public override void SendChatRoomMessage(object sender, MessageProcessEventData messagedata)
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

        

        public override void SendPrivateMessage(object sender, MessageProcessEventData messagedata)
        {
            try
            {
                User user = (User)messagedata.Sender.identifier;
                user.SendMessage(messagedata.ReplyMessage);
            }
            catch
            {

            }
        }

        public override void SetUsername(object sender, string Username)
        {
          //  _client.SetGame(Username);
        }
        


        public override void tick()
        {
            
        }

        public override void BroadCastMessage(object sender, string message)
        {
                Channel channel = (Channel)MainChatRoom.identifier;
                channel.SendMessage(message);
        }
    }
}
