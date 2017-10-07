using System;

namespace SteamBotLite
{
    internal class ConsoleUserHandler : UserHandler
    {
        public override void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e)
        {
        }

        public override void OnLoginCompleted(object sender, EventArgs e)
        {
        }

        public override void ProcessChatRoomMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine("{0} | {1}: {2}", e.Chatroom, e.Sender.DisplayName, e.ReceivedMessage);
        }

        public override void ProcessPrivateMessage(object sender, MessageEventArgs e)
        {
            Console.WriteLine("{0}: {1}", e.Sender.DisplayName, e.ReceivedMessage);
        }
    }
}