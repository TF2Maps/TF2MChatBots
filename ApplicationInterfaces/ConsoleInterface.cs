using System;

namespace SteamBotLite
{
    internal class ConsoleInterface : ApplicationInterface
    {
        public override void BroadCastMessage(object sender, string message)
        {
        }

        public override void EnterChatRoom(object sender, ChatroomEntity ChatroomEntity)
        {
        }

        public override string GetOthersUsername(object sender, ChatroomEntity user)
        {
            return null;
        }

        public override string GetUsername()
        {
            return null;
        }

        public override void LeaveChatroom(object sender, ChatroomEntity ChatroomEntity)
        {
        }

        public override void Reboot(object sender, EventArgs e)
        {
        }

        public override void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus)
        {
        }

        public override void SendChatRoomMessageAsync(object sender, MessageEventArgs messagedata)
        {
            Console.WriteLine(messagedata.ReplyMessage);
        }

        public override void SendPrivateMessage(object sender, MessageEventArgs messagedata)
        {
            Console.WriteLine(messagedata.ReplyMessage);
        }

        public override void SetStatusMessage(object sender, string message)
        {
        }

        public override void SetUsername(object sender, string Username)
        {
        }

        public override void tick()
        {
        }
    }
}