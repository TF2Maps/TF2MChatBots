using System;

namespace SteamBotLite
{
    public abstract class UserHandler
    {
        public EventHandler<Tuple<ChatroomEntity, bool>> ChatMemberInfoEvent;

        /// <summary>
        /// Sets the SteamConnectionHandler to Bot
        /// </summary>
        /// <param name="SteamConnectionHandler"></param>
        public UserHandler()
        {
        }

        public event EventHandler<string> BroadcastMessageEvent;

        public event EventHandler<ChatroomEntity> ChatRoomJoin;

        public event EventHandler<ChatroomEntity> ChatRoomLeave;

        public event EventHandler<ChatroomEntity> ChatRoomOther;

        public event EventHandler<EventArgs> MainChatRoomJoin;

        public event EventHandler<EventArgs> MainChatRoomLeave;

        public event EventHandler<EventArgs> MainChatRoomOther;

        public event EventHandler<EventArgs> RebootEvent;

        public event EventHandler<MessageEventArgs> SendChatRoomMessageEvent;

        public event EventHandler<MessageEventArgs> SendPrivateMessageEvent;

        public event EventHandler<string> SetStatusmessage;

        public event EventHandler<string> SetUsernameEvent;

        public enum ChatroomEventEnum { LeaveChat, EnterChat, Other };

        public void AssignAppInterface(ApplicationInterface appinterface)
        {
            appinterface.AnnounceLoginCompletedEvent += OnLoginCompleted;
            appinterface.PrivateMessageEvent += ProcessPrivateMessage;
            appinterface.ChatRoomMessageEvent += ProcessChatRoomMessage;
            appinterface.ChatMemberInfoEvent += ChatMemberInfo;
        }

        //The event-invoking method that derived classes can override.
        public virtual void BroadcastMessageProcessEvent(string message)
        {
            EventHandler<string> handler = BroadcastMessageEvent;
            if (handler != null)
            {
                handler(this, message);
            }
        }

        public abstract void ChatMemberInfo(object sender, Tuple<ChatroomEntity, bool> e);

        //The event-invoking method that derived classes can override.
        public virtual void FireMainChatRoomEvent(ChatroomEventEnum e)
        {
            EventHandler<EventArgs> handler;
            switch (e)
            {
                case ChatroomEventEnum.EnterChat:
                    handler = MainChatRoomJoin;
                    break;

                case ChatroomEventEnum.LeaveChat:
                    handler = MainChatRoomLeave;
                    break;

                case ChatroomEventEnum.Other:
                    handler = MainChatRoomOther;
                    break;

                default:
                    handler = null;
                    break;
            }
            if (handler != null)
            {
                handler(this, null);
            }
        }

        public abstract void OnLoginCompleted(object sender, EventArgs e);

        public abstract void ProcessChatRoomMessage(object sender, MessageEventArgs e);

        public abstract void ProcessPrivateMessage(object sender, MessageEventArgs e);

        //TODO make this an object, not a bool
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

        //The event-invoking method that derived classes can override.
        public virtual void SendChatRoomMessageProcessEvent(MessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageEventArgs> handler = SendChatRoomMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //The event-invoking method that derived classes can override.
        public virtual void SendPrivateMessageProcessEvent(MessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageEventArgs> handler = SendPrivateMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //The event-invoking method that derived classes can override.
        public virtual void SetStatusmessageEvent(string e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<string> handler = SetStatusmessage;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //The event-invoking method that derived classes can override.
        public virtual void SetUsernameEventProcess(string e)
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

        //The event-invoking method that derived classes can override.
        protected virtual void FireChatRoomEvent(ChatroomEventEnum e, ChatroomEntity chatroom)
        {
            EventHandler<ChatroomEntity> handler;
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
    }
}