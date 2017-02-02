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
            appinterface.ChatMemberInfoEvent += ChatMemberInfo;
        }

        public abstract void ProcessChatRoomMessage(object sender, MessageEventArgs e);

        public abstract void ProcessPrivateMessage(object sender, MessageEventArgs e);

        public abstract void OnLoginCompleted(object sender, EventArgs e);


        public EventHandler<Tuple<ChatroomEntity, bool>> ChatMemberInfoEvent;

        public abstract void ChatMemberInfo(object sender , Tuple<ChatroomEntity,bool> e); //TODO make this an object, not a bool

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

        public enum ChatroomEventEnum { LeaveChat, EnterChat , Other };


        public event EventHandler<ChatroomEntity> ChatRoomJoin;
        public event EventHandler<ChatroomEntity> ChatRoomLeave;
        public event EventHandler<ChatroomEntity> ChatRoomOther;

        //The event-invoking method that derived classes can override.
        protected virtual void FireChatRoomEvent(ChatroomEventEnum e , ChatroomEntity chatroom)
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

        public event EventHandler<EventArgs> MainChatRoomJoin;
        public event EventHandler<EventArgs> MainChatRoomLeave;
        public event EventHandler<EventArgs> MainChatRoomOther;
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

        public event EventHandler<string> BroadcastMessageEvent;

        //The event-invoking method that derived classes can override.
        public virtual void BroadcastMessageProcessEvent(string message)
        {
            EventHandler<string> handler = BroadcastMessageEvent;
            if (handler != null)
            {
                handler(this, message);
            }
        }

        public event EventHandler<MessageEventArgs> SendPrivateMessageEvent;

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

        public event EventHandler<MessageEventArgs> SendChatRoomMessageEvent;

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

        public event EventHandler<string> SetStatusmessage;

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



    }
}
