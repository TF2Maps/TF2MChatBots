using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamKit2;

namespace SteamBotLite
{
    public abstract class UserHandler
    {
       
        /// <summary>
        /// Sets the SteamConnectionHandler to Bot
        /// </summary>
        /// <param name="SteamConnectionHandler"></param>
        public UserHandler()
        {
        }

        public void AssignAppInterface (ApplicationInterface appinterface)
        {
            appinterface.AnnounceLoginCompletedEvent += OnLoginCompleted;
            appinterface.PrivateMessageEvent += ProcessPrivateMessage;
            appinterface.ChatRoomMessageEvent += ProcessChatRoomMessage;
            Console.WriteLine("Reminder That your Reboot method has not been implemented");
        }

        public abstract void ProcessChatRoomMessage(object sender, MessageProcessEventData e);

        public abstract void ProcessPrivateMessage(object sender, MessageProcessEventData e);

        public abstract void OnLoginCompleted(object sender, EventArgs e);

        
        public abstract void ChatMemberInfo(UserIdentifier useridentifier, bool MemberInfo); //TODO make this an object, not a bool

        public event EventHandler<EventArgs> RebootEvent;
        /// <summary>
        /// Reboot the connection with steam
        /// </summary>
        public void Reboot()
        {
            EventHandler<EventArgs> handler = RebootEvent;
            if (handler != null)
            {
                handler(this, null);
            }
        }


        public event EventHandler<string> SetUsernameEvent;

        //The event-invoking method that derived classes can override.
        protected virtual void SetUsernameEventProcess(string e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<string> handler = SetUsernameEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public enum ChatroomEventEnum { LeaveChat, EnterChat , Other };


        public event EventHandler<ChatRoomIdentifier> ChatRoomJoin;
        public event EventHandler<ChatRoomIdentifier> ChatRoomLeave;
        public event EventHandler<ChatRoomIdentifier> ChatRoomOther;

        //The event-invoking method that derived classes can override.
        protected virtual void FireChatRoomEvent(ChatroomEventEnum e , ChatRoomIdentifier chatroom)
        {
            EventHandler<ChatRoomIdentifier> handler;
            switch (e)
            {
                case ChatroomEventEnum.EnterChat:
                    handler = ChatRoomJoin;
                    break;
                case ChatroomEventEnum.LeaveChat:
                    handler = ChatRoomLeave;
                    break;
                case ChatroomEventEnum.Other:
                    handler = ChatRoomOther;
                    break;
                default:
                    handler = null;
                    break;
            }
            if (handler != null)
            {
                handler(this, chatroom);
            }
        }

        public event EventHandler<MessageProcessEventData> SendPrivateMessageEvent;

        //The event-invoking method that derived classes can override.
        public virtual void SendPrivateMessageProcessEvent(MessageProcessEventData e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageProcessEventData> handler = SendPrivateMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<MessageProcessEventData> SendChatRoomMessageEvent;

        //The event-invoking method that derived classes can override.
        public virtual void SendChatRoomMessageProcessEvent(MessageProcessEventData e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageProcessEventData> handler = SendChatRoomMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }






    }
}
