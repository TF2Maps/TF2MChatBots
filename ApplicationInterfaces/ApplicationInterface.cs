using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SteamBotLite
{
    
    public struct ChatroomEntity
    {
        public enum Individual {User, Chatroom };
        public enum AdminStatus { Unknown, Other, False, True};
        public ApplicationInterface Application;
        public object identifier;
        public Individual EntityClass;
        public AdminStatus Rank;
        public string DisplayName;
        public object ExtraData;

        public ChatroomEntity(object identifier, Individual EntityClassification, ApplicationInterface Application, string DisplayName = "", AdminStatus Rank = AdminStatus.Unknown , object ParentIdentifier = null, object ExtraData = null)
        {
            this.identifier = identifier;
            this.EntityClass = EntityClassification;
            this.Rank = Rank;
            this.Application = Application;
            this.DisplayName = DisplayName;
            if (ParentIdentifier != null)
            {
                this.ParentIdentifier = ParentIdentifier;
                IsChild = true;
            }
            else
            {
                this.ParentIdentifier = null;
                IsChild = false;
            }
            this.ExtraData = ExtraData;
            
        }
        
        
        public object ParentIdentifier;
        bool IsChild;
    }

    public class MessageEventArgs : EventArgs
    {
        public ChatroomEntity Sender;
        public ChatroomEntity Destination;
        public ChatroomEntity Chatroom;
        public string ReceivedMessage;
        public string ReplyMessage;
        public ApplicationInterface InterfaceHandlerDestination;    
        public MessageEventArgs (ApplicationInterface interfacehandlerdestination)
        {
            InterfaceHandlerDestination = interfacehandlerdestination;
        }
    }

        public abstract class ApplicationInterface
    {

        public ChatroomEntity MainChatRoom;
        public Dictionary<string, object> config;
        

        public ApplicationInterface()
        {
            this.config = JsonConvert.DeserializeObject<Dictionary<string, object>>(System.IO.File.ReadAllText(Path.Combine("applicationconfigs" , this.GetType().Name.ToString() + ".json")));
        }


        public abstract void SendChatRoomMessage(object sender, MessageEventArgs messagedata);
        public abstract void SendPrivateMessage(object sender, MessageEventArgs messagedata);
        public abstract void BroadCastMessage(object sender, string message);


        public void AssignUserHandler(UserHandler userhandler)
        {
            userhandler.ChatRoomJoin += EnterChatRoom;
            userhandler.ChatRoomLeave += LeaveChatroom;
            userhandler.SendChatRoomMessageEvent += SendChatRoomMessage;
            userhandler.SendPrivateMessageEvent += SendPrivateMessage;
            userhandler.SetUsernameEvent += SetUsername;
            userhandler.RebootEvent += Reboot;
            userhandler.BroadcastMessageEvent += BroadCastMessage;
            userhandler.MainChatRoomJoin += EnterMainChatRoom;
            userhandler.MainChatRoomLeave += LeaveMainChatroom;
            userhandler.ChatMemberInfoEvent += ChatMemberInfoEvent;


        }


        /* I beleive there is no benefit to making this role mandatory
        public abstract void ReceiveChatRoomMessage(ChatroomEntity ChatroomEntity, string Message);
        public abstract void ReceivePrivateMessage(ChatroomEntity ChatroomEntity, string Message);
        */

        public class ChatMemberInfoEventArgs
        {
            ChatroomEntity User;
            bool IsAdmin;
        }

        /*
        public event EventHandler<ChatMemberInfoEventArgs> ChatMemberInfoEvent;
        protected virtual void ChatMemberInfoProcessEvent(ChatMemberInfoEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<ChatMemberInfoEventArgs> handler = ChatMemberInfoEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        */
        public event EventHandler<MessageEventArgs> ChatRoomMessageEvent;
        
        //The event-invoking method that derived classes can override.
        protected virtual void ChatRoomMessageProcessEvent(MessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageEventArgs> handler = ChatRoomMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<MessageEventArgs> PrivateMessageEvent;

        //The event-invoking method that derived classes can override.
        protected virtual void PrivateMessageProcessEvent(MessageEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<MessageEventArgs> handler = PrivateMessageEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        

        public event EventHandler<Tuple<ChatroomEntity, bool>> ChatMemberInfoEvent;
        //The event-invoking method that derived classes can override.
        protected virtual void ChatMemberInfoProcessEvent(ChatroomEntity e , bool isadmin)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler<Tuple<ChatroomEntity, bool>> handler = ChatMemberInfoEvent;
            if (handler != null)
            {
                handler(this, new Tuple<ChatroomEntity, bool>(e,isadmin));
            }
        }

        public event EventHandler AnnounceLoginCompletedEvent;

        protected virtual void AnnounceLoginCompleted()
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            EventHandler handler = AnnounceLoginCompletedEvent;
            if (handler != null)
            {
                AnnounceLoginCompletedEvent(this , null);
            }
        }

        public abstract void ReceiveChatMemberInfo(ChatroomEntity ChatroomEntity, bool AdminStatus);

        public abstract void EnterChatRoom (object sender, ChatroomEntity ChatroomEntity);
        public abstract void LeaveChatroom (object sender, ChatroomEntity ChatroomEntity);

        public void EnterMainChatRoom(object sender, EventArgs e)
        {
            EnterChatRoom(sender, MainChatRoom);
        }

        public void LeaveMainChatroom(object sender, EventArgs e)
        {
            LeaveChatroom(sender, MainChatRoom);
        }

        public abstract void Reboot(object sender, EventArgs e);

        public abstract void SetUsername(object sender, string Username);
        public abstract string GetUsername();

        public abstract string GetOthersUsername(object sender, ChatroomEntity user);

        public abstract void tick();

        public enum TickThreadState { Running , Stopped};

        public TickThreadState TickThread = TickThreadState.Running;

        public void StartTickThreadLoop()
        {
            while (TickThread == TickThreadState.Running)
            {
                tick();
            }
        }

        public string Username
        {
            get
            {
                return GetUsername();
            }

            set
            {
               SetUsername(this, value);
            }
        }
    }
}
